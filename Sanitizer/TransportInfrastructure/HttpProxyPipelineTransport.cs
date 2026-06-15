using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;

namespace Sanitizer.TransportInfrastructure
{
    public class HttpProxyPipelineTransport(IWebProxy proxy, int networkTimeoutInSeconds = 600) : PipelineTransport
    {
        protected override PipelineMessage CreateMessageCore()
        {
            PipelineRequest request = new HttpPipelineRequest();

            var pipelineMessageType = typeof(PipelineMessage);

            var constructor = pipelineMessageType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                [typeof(PipelineRequest)],
                null
            );

            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            var message = (PipelineMessage)constructor.Invoke([request]);
            return message;
        }

        protected sealed override void ProcessCore(PipelineMessage message)
            => ProcessSyncOrAsync(message).ConfigureAwait(false).GetAwaiter().GetResult();

        protected sealed override async ValueTask ProcessCoreAsync(PipelineMessage message)
            => await ProcessSyncOrAsync(message).ConfigureAwait(false);

        private async ValueTask ProcessSyncOrAsync(PipelineMessage message)
        {
            using var httpClient = CreateHttpClient();

            if (message.Request is not PipelineRequest request)
                throw new InvalidOperationException($"The request type is not compatible with the transport: '{message.Request?.GetType()}'.");

            using var httpRequest = HttpPipelineRequest.BuildHttpRequestMessage(request, message.CancellationToken);

            SetMessageResponse(message, null);

            var responseMessage = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, message.CancellationToken).ConfigureAwait(false);

            Stream? contentStream = null;

            if (responseMessage.Content != null)
                contentStream = await responseMessage.Content.ReadAsStreamAsync(message.CancellationToken).ConfigureAwait(false);

            SetMessageResponse(message, responseMessage);

            if (contentStream is not null && message.Response is not null)
                message.Response.ContentStream = contentStream;
        }

        private void EnsureProxyIsSet(HttpClient httpClient)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            var handlerField = typeof(HttpMessageInvoker).GetField("_handler", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Unable to access the handler field in HttpClient.");
            var handler = handlerField.GetValue(httpClient);

            if (handler is HttpClientHandler httpClientHandler)
            {
                if (httpClientHandler.Proxy is null)
                    throw new InvalidOperationException("HttpClient does not have a proxy configured.");
            }
            else
            {
                throw new InvalidOperationException("Handler is not HttpClientHandler, or proxy cannot be checked.");
            }
        }

        private void SetMessageResponse(PipelineMessage message, HttpResponseMessage? response)
        {
            Type pipelineMessageType = typeof(PipelineMessage);

            PropertyInfo? responseProperty = pipelineMessageType.GetProperty(
                "Response", BindingFlags.Public | BindingFlags.Instance);

            if (responseProperty == null)
                throw new ArgumentNullException(nameof(responseProperty));

            HttpClientTransportResponse? responseToSet = null;

            if (response is not null)
                responseToSet = new HttpClientTransportResponse(response);

            responseProperty.SetValue(message, responseToSet);
        }

        private HttpClient CreateHttpClient()
        {
            if (proxy is null)
                throw new ArgumentException(nameof(proxy));

            return CreateHttpClient(proxy);
        }

        private HttpClient CreateHttpClient(IWebProxy proxy)
        {
            var handler = new HttpClientHandler()
            {
                Proxy = proxy,
                AllowAutoRedirect = false
            };

            ServicePointHelpers.SetLimits(handler);

            var httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(networkTimeoutInSeconds)
            };

            EnsureProxyIsSet(httpClient);

            return httpClient;
        }

        private class HttpPipelineRequest : PipelineRequest
        {
            private const string AuthorizationHeaderName = "Authorization";

            private string _method;
            private Uri? _uri;
            private BinaryContent? _content;

            private readonly PipelineRequestHeaders _headers;

            private bool _disposed;

            protected internal HttpPipelineRequest()
            {
                _method = HttpMethod.Get.Method;
                _headers = new ArrayBackedRequestHeaders();
            }

            protected override string MethodCore
            {
                get => _method;
                set
                {
                    if (value is null)
                        throw new ArgumentNullException(nameof(value));

                    _method = value;
                }
            }

            protected override Uri? UriCore
            {
                get => _uri;
                set
                {
                    if (value is null)
                        throw new ArgumentNullException(nameof(value));

                    _uri = value;
                }
            }

            protected override BinaryContent? ContentCore
            {
                get => _content;
                set => _content = value;
            }

            protected override PipelineRequestHeaders HeadersCore => _headers;

            private static readonly HttpMethod _patchMethod = new HttpMethod("PATCH");

            private static HttpMethod ToHttpMethod(string method)
            {
                return method switch
                {
                    "GET" => HttpMethod.Get,
                    "POST" => HttpMethod.Post,
                    "PUT" => HttpMethod.Put,
                    "HEAD" => HttpMethod.Head,
                    "DELETE" => HttpMethod.Delete,
                    "PATCH" => _patchMethod,
                    _ => new HttpMethod(method),
                };
            }

            internal static HttpRequestMessage BuildHttpRequestMessage(PipelineRequest request, CancellationToken cancellationToken)
            {
                if (request.Uri is null)
                {
                    throw new InvalidOperationException("Uri must be set on message request prior to sending message.");
                }

                HttpMethod method = ToHttpMethod(request.Method);
                Uri uri = request.Uri;
                HttpRequestMessage httpRequest = new HttpRequestMessage(method, uri);

                MessageBodyAdapter? httpContent = request.Content == null ? null :
                    new MessageBodyAdapter(request.Content, cancellationToken);
                httpRequest.Content = httpContent;
#if NETSTANDARD
            httpRequest.Headers.ExpectContinue = false;
#endif

                if (request.Headers is not ArrayBackedRequestHeaders headers)
                {
                    throw new InvalidOperationException($"Invalid type for request.Headers: '{request.Headers?.GetType()}'.");
                }

                int i = 0;
                while (headers.GetNextValue(i++, out string headerName, out object? headerValue))
                {
                    switch (headerValue)
                    {
                        case string stringValue:
                            // Authorization is special cased because it is in the hot path for auth polices that set this header on each request and retry.
                            if (headerName == AuthorizationHeaderName && AuthenticationHeaderValue.TryParse(stringValue, out var authHeader))
                            {
                                httpRequest.Headers.Authorization = authHeader;
                            }
                            else if (!httpRequest.Headers.TryAddWithoutValidation(headerName, stringValue))
                            {
                                if (httpContent != null && !httpContent.Headers.TryAddWithoutValidation(headerName, stringValue))
                                {
                                    throw new InvalidOperationException($"Unable to add header {headerName} to header collection.");
                                }
                            }
                            break;

                        case List<string> listValue:
                            if (!httpRequest.Headers.TryAddWithoutValidation(headerName, listValue))
                            {
                                if (httpContent != null && !httpContent.Headers.TryAddWithoutValidation(headerName, listValue))
                                {
                                    throw new InvalidOperationException($"Unable to add header {headerName} to header collection.");
                                }
                            }
                            break;
                    }
                }

                return httpRequest;
            }

            private sealed class MessageBodyAdapter : HttpContent
            {
                private readonly BinaryContent _content;
                private readonly CancellationToken _cancellationToken;

                public MessageBodyAdapter(BinaryContent content, CancellationToken cancellationToken)
                {
                    if (content is null)
                        throw new ArgumentNullException(nameof(content));

                    _content = content;
                    _cancellationToken = cancellationToken;
                }

                protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
                    => await _content.WriteToAsync(stream, _cancellationToken).ConfigureAwait(false);

                protected override bool TryComputeLength(out long length)
                    => _content.TryComputeLength(out length);

                protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
                    => await _content!.WriteToAsync(stream, cancellationToken).ConfigureAwait(false);

                protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken)
                    => _content.WriteTo(stream, cancellationToken);
            }

            public override string ToString() => BuildHttpRequestMessage(this, default).ToString();

            public sealed override void Dispose()
            {
                Dispose(true);

                GC.SuppressFinalize(this);
            }

            protected void Dispose(bool disposing)
            {
                if (disposing && !_disposed)
                {
                    var content = _content;
                    if (content != null)
                    {
                        _content = null;
                        content.Dispose();
                    }

                    _disposed = true;
                }
            }
        }

        private class HttpClientTransportResponse : PipelineResponse
        {
            private readonly HttpResponseMessage _httpResponse;

            // We keep a reference to the http response content so it will be available
            // for reading headers, even if we set _httpResponse.Content to null when we
            // buffer the content.  Since we handle disposing the content separately, we
            // don't believe there is a concern about rooting objects that are holding
            // references to network resources.
            private readonly HttpContent _httpResponseContent;

            private Stream? _contentStream;
            private BinaryData? _bufferedContent;

            private bool _disposed;

            public HttpClientTransportResponse(HttpResponseMessage httpResponse)
            {
                _httpResponse = httpResponse ?? throw new ArgumentNullException(nameof(httpResponse));
                _httpResponseContent = _httpResponse.Content;

                // Don't dispose the content so it remains available for reading headers.
                _httpResponse.Content = null;
            }

            public override int Status => (int)_httpResponse.StatusCode;

            public override string ReasonPhrase
                => _httpResponse.ReasonPhrase ?? string.Empty;

            protected override PipelineResponseHeaders HeadersCore
                => new HttpClientResponseHeaders(_httpResponse, _httpResponseContent);

            public override Stream? ContentStream
            {
                get
                {
                    if (_contentStream is not null)
                    {
                        return _contentStream;
                    }

                    return BufferContent().ToStream();
                }
                set
                {
                    _contentStream = value;

                    // Invalidate the cache since the source-stream has been replaced.
                    _bufferedContent = null;
                }
            }

            public override BinaryData Content
            {
                get
                {
                    if (_bufferedContent is not null)
                    {
                        return _bufferedContent;
                    }

                    if (_contentStream is null || _contentStream is MemoryStream)
                    {
                        return BufferContent();
                    }

                    throw new InvalidOperationException($"The response is not buffered.");
                }
            }

            public override BinaryData BufferContent(CancellationToken cancellationToken = default)
                => BufferContentSyncOrAsync(cancellationToken, async: false).ConfigureAwait(false).GetAwaiter().GetResult();

            public override async ValueTask<BinaryData> BufferContentAsync(CancellationToken cancellationToken = default)
                => await BufferContentSyncOrAsync(cancellationToken, async: true).ConfigureAwait(false);

            private async ValueTask<BinaryData> BufferContentSyncOrAsync(CancellationToken cancellationToken, bool async)
            {
                if (_bufferedContent is not null)
                {
                    // Content has already been buffered.
                    return _bufferedContent;
                }

                if (_contentStream == null)
                {
                    // Content is not buffered but there is no source stream.
                    // Our contract from Azure.Core is to return BinaryData.Empty in this case.
                    _bufferedContent = new(Array.Empty<byte>());
                    return _bufferedContent;
                }

                if (_contentStream.CanSeek && _contentStream.Position != 0)
                {
                    throw new InvalidOperationException("Content stream position is not at beginning of stream.");
                }

                // ContentStream still holds the source stream.  Buffer the content
                // and dispose the source stream.
                BufferedContentStream bufferStream = new();

                if (async)
                {
                    await _contentStream.CopyToAsync(bufferStream, cancellationToken).ConfigureAwait(false);
#if NETSTANDARD2_0
                _contentStream.Dispose();
#else
                    await _contentStream.DisposeAsync().ConfigureAwait(false);
#endif
                }
                else
                {
                    _contentStream.CopyTo(bufferStream);
                    _contentStream.Dispose();
                }

                _contentStream = null;

                bufferStream.Position = 0;

                _bufferedContent = bufferStream.TryGetBuffer(out ArraySegment<byte> segment) ?
                    new BinaryData(segment.AsMemory()) :
                    new BinaryData(bufferStream.ToArray());

                return _bufferedContent;
            }

            public override void Dispose()
            {
                Dispose(true);

                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing && !_disposed)
                {
                    HttpResponseMessage httpResponse = _httpResponse;
                    httpResponse?.Dispose();

                    if (ContentStream is MemoryStream)
                    {
                        BufferContent();
                    }

                    Stream? contentStream = _contentStream;
                    contentStream?.Dispose();
                    _contentStream = null;

                    _disposed = true;
                }
            }

            private class BufferedContentStream : MemoryStream { }
        }
    }
}
