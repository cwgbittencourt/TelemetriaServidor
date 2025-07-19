using System.Management;
/// <summary>
/// @copilot: add an xml-doc comment including param and returns tags
/// </summary>
namespace TelemetriaServidor
{
    /// <summary>
    /// @copilot: add an xml-doc comment including param and returns tags
    /// </summary>
    public static class MachineInfo
    {
        /// <summary>
        /// Total de RAM física em MB
        /// @copilot: add an xml-doc comment including param and returns tags
        /// </summary>
        public static readonly double TotalPhysicalMemoryMb;

        /// <summary>
        /// @copilot: add an xml-doc comment including param and returns tags
        /// </summary>
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
