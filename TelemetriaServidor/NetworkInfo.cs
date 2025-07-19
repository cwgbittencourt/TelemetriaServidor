using System.Net;
using System.Net.Sockets;

/// <summary>
/// @copilot: add an xml-doc comment including param and returns tags
/// </summary>
namespace TelemetriaServidor
{
    /// <summary>
    /// @copilot: add an xml-doc comment including param and returns tags
    /// </summary>
    public static class NetworkInfo
    {
        /// <summary>
        /// @copilot: add an xml-doc comment including param and returns tags
        /// </summary>
        public static string GetLocalIPv4()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ip = host.AddressList
                         .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork
                                           && !IPAddress.IsLoopback(a));
            return ip?.ToString() ?? "0.0.0.0";
        }
    }
}
