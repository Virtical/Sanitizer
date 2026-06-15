using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sanitizer.TransportInfrastructure
{
    internal static class ServicePointHelpers
    {
        private const int RuntimeDefaultConnectionLimit = 2;
        private const int IncreasedConnectionLimit = 50;
        private const int IncreasedConnectionLeaseTimeout = 300 * 1000;

#pragma warning disable CA1823 // Unused field
        private static TimeSpan DefaultConnectionLeaseTimeoutTimeSpan = Timeout.InfiniteTimeSpan;
        private static TimeSpan IncreasedConnectionLeaseTimeoutTimeSpan = TimeSpan.FromMilliseconds(IncreasedConnectionLeaseTimeout);
#pragma warning restore
        
        public static void SetLimits(HttpMessageHandler messageHandler)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
            {
                return;
            }

            try
            {
                switch (messageHandler)
                {
                    case HttpClientHandler httpClientHandler:
                        // Only change when the default runtime limit is used
                        if (httpClientHandler.MaxConnectionsPerServer == RuntimeDefaultConnectionLimit)
                        {
                            httpClientHandler.MaxConnectionsPerServer = IncreasedConnectionLimit;
                        }
                        break;
#if NETCOREAPP
                    case SocketsHttpHandler socketsHttpHandler:
                        if (socketsHttpHandler.MaxConnectionsPerServer == RuntimeDefaultConnectionLimit)
                        {
                            socketsHttpHandler.MaxConnectionsPerServer = IncreasedConnectionLimit;
                        }
                        if (socketsHttpHandler.PooledConnectionLifetime == DefaultConnectionLeaseTimeoutTimeSpan)
                        {
                            socketsHttpHandler.PooledConnectionLifetime = IncreasedConnectionLeaseTimeoutTimeSpan;
                        }
                        break;
#endif
                    default:
                        Debug.Assert(false, "Unknown handler type");
                        break;
                }
            }
            catch (NotSupportedException)
            {
                // Some platforms might throw NotSupportedException
                // when accessing handler options
            }
            catch (NotImplementedException)
            {
                // Some platforms (like Unity) might throw NotImplementedException
                // when accessing handler options
            }
        }
    }
}
