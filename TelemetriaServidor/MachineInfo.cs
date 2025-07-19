using System.Management;

namespace TelemetriaServidor
{
    /// <summary>
    /// Classe utilitária para obter informações da máquina.
    /// </summary>
    public static class MachineInfo
    {
        /// <summary>
        /// Obtém o total de RAM física disponível na máquina, em megabytes (MB).
        /// </summary>
        /// <remarks>
        /// O valor é inicializado no carregamento da classe, utilizando a consulta WMI.
        /// </remarks>
        public static readonly double TotalPhysicalMemoryMb;

        /// <summary>
        /// Inicializa o valor de <see cref="TotalPhysicalMemoryMb"/> consultando a quantidade total de memória física.
        /// </summary>
        /// <returns>
        /// Não retorna valor diretamente. Inicializa o campo <see cref="TotalPhysicalMemoryMb"/> com o valor obtido ou 0 em caso de erro.
        /// </returns>
        static MachineInfo()
        {
            try
            {
                var searcher = new ManagementObjectSearcher(
                    "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                var mos = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (mos != null && ulong.TryParse(mos["TotalPhysicalMemory"]?.ToString(), out var bytes))
                {
                    TotalPhysicalMemoryMb = bytes / 1024.0 / 1024.0;
                }
            }
            catch
            {
                TotalPhysicalMemoryMb = 0;
            }
        }
    }
}
