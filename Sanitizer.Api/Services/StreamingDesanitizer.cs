using System.Text;
using Sanitizer.Api.Models;

namespace Sanitizer.Api.Services;

/// <summary>
/// Потоковый детокенизатор: принимает чанки ответа LLM, удерживает в буфере
/// только незакрытые токены формата [TYPE_N] и отдаёт безопасные части
/// клиенту по мере поступления.
/// </summary>
public sealed class StreamingDesanitizer(DesanitizerService inner, SanitizationContext context)
{
    private const int MaxTokenLength = 15;

    private readonly StringBuilder _buffer = new();

    /// <summary>Принимает чанк, возвращает безопасную часть для отправки клиенту.</summary>
    public string Push(string chunk)
    {
        _buffer.Append(chunk);

        int openIdx = LastIndexOf(_buffer, '[');

        if (openIdx < 0)
            return Drain();

        int closeIdx = IndexOf(_buffer, ']', openIdx + 1);

        if (closeIdx >= 0)
        {
            var replaced = inner.Desanitize(_buffer.ToString(), context);
            _buffer.Clear();
            return replaced;
        }

        if (_buffer.Length - openIdx > MaxTokenLength)
            return Drain();

        if (openIdx == 0) return string.Empty;

        var safe = _buffer.ToString(0, openIdx);
        _buffer.Remove(0, openIdx);
        return safe;
    }

    /// <summary>Возвращает оставшийся хвост (вызывать в конце стрима).</summary>
    public string Flush()
    {
        if (_buffer.Length == 0) return string.Empty;
        var tail = inner.Desanitize(_buffer.ToString(), context);
        _buffer.Clear();
        return tail;
    }

    private string Drain()
    {
        var s = _buffer.ToString();
        _buffer.Clear();
        return s;
    }

    private static int LastIndexOf(StringBuilder sb, char c)
    {
        for (int i = sb.Length - 1; i >= 0; i--)
            if (sb[i] == c) return i;
        return -1;
    }

    private static int IndexOf(StringBuilder sb, char c, int start)
    {
        for (int i = start; i < sb.Length; i++)
            if (sb[i] == c) return i;
        return -1;
    }
}
