using Microsoft.Extensions.Options;
using System.Diagnostics;
using TelemetriaServidor.Interface;
using TelemetriaServidor.Model;

/// <summary>
/// @copilot: add an xml-doc comment including param and returns tags
/// </summary>
namespace TelemetriaServidor.Service
{
    /// <summary>
    /// @copilot: add an xml-doc comment including param and returns tags
    /// </summary>
    public class MetricsProvider : IMetricsProvider, IDisposable
    {
        /// <summary>
        /// @copilot: add an xml-doc comment including param and returns tags
        /// </summary>
        public IReadOnlyDictionary<string, (PerformanceCounter, string)> Counters { get; }

        /// <summary>
        /// @copilot: add an xml-doc comment including param and returns tags
        /// </summary>
        public MetricsProvider(IOptions<TelemetryOptions> opts)
        {
            var dict = new Dictionary<string, (PerformanceCounter, string)>();
            foreach (var def in opts.Value.Metrics)
            {
                if (!PerformanceCounterCategory.Exists(def.Category) ||
                    !PerformanceCounterCategory.CounterExists(def.Counter, def.Category))
                    continue;

                var inst = def.Instance ?? string.Empty;
                // Se instância não existe, tentar usar primeira disponível
                if (!string.IsNullOrWhiteSpace(inst))
                {
                    var cat = new PerformanceCounterCategory(def.Category);
                    if (!cat.GetInstanceNames().Contains(inst))
                        inst = cat.GetInstanceNames().FirstOrDefault() ?? inst;
                }

                var pc = new PerformanceCounter(def.Category, def.Counter, inst);
                dict[def.MetricName] = (pc, def.MetricType);
            }
            Counters = dict;
        }

        /// <summary>
        /// @copilot: add an xml-doc comment including param and returns tags
        /// </summary>
        public void Dispose()
        {
            foreach (var (pc, _) in Counters.Values)
                pc.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
