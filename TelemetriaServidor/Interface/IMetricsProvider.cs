using System.Diagnostics;
/// <summary>
/// @copilot: add an xml-doc comment including param and returns tags
/// </summary>
namespace TelemetriaServidor.Interface
{
    /// <summary>
    /// @copilot: add an xml-doc comment including param and returns tags
    /// </summary>
    public interface IMetricsProvider
    {
        IReadOnlyDictionary<string, (PerformanceCounter Counter, string MetricType)> Counters { get; }
    }
}
