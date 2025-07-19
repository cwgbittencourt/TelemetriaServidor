/// <summary>
/// @copilot: add an xml-doc comment including param and returns tags
/// </summary>
namespace TelemetriaServidor.Model
{
    /// <summary>
    /// @copilot: add an xml-doc comment including param and returns tags
    /// </summary>
    public class MetricDefinitionModel
    {
        /// <summary>
        /// Categoria conforme PerformanceCounterCategory (ex: "Processor", "Memory", "Network Interface"…)
        /// </summary>
        public string Category { get; set; } = "";

        /// <summary>
        /// Nome do contador (ex: "% Processor Time", "Available MBytes", "Bytes Sent/sec"…)
        /// </summary>
        public string Counter { get; set; } = "";

        /// <summary>
        /// Nome da instância (ex: "_Total", ou nome da NIC, ou nome do processo)
        /// </summary>
        public string? Instance { get; set; }

        /// <summary>
        /// Nome amigável para logar e identificar (ex: "CpuTotal", "MemAvailable", "NetBytesSent"…)
        /// </summary>
        public string MetricName { get; set; } = "";

        /// <summary>
        /// Tipo de métrica, para agrupar depois no Seq (ex: "CPU", "Memória", "Rede", "Disk"…)
        /// </summary>
        public string MetricType { get; set; } = "";
    }
}
