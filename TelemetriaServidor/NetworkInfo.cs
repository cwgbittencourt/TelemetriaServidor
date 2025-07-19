using System.Net;
using System.Net.Sockets;

/// <summary>
/// @copilot: add an xml-doc comment including param and returns tags
/// </summary>
namespace TelemetriaServidor
{
    /// <summary>
    /// Classe utilitária para obter informações de rede do servidor.
    /// </summary>
    public static class NetworkInfo
    {
        /// <summary>
        /// Obtém o endereço IPv4 local da máquina, excluindo endereços de loopback.
        /// </summary>
        /// <param name="none">Este método não recebe parâmetros.</param>
        /// <returns>
        /// Uma string representando o endereço IPv4 local, ou "0.0.0.0" se não for encontrado.
        /// </returns>
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
