namespace TelemetriaServidor.Model
{
    public class TelemetryOptions
    {
        public int IntervalSeconds { get; set; }
        public List<MetricDefinitionModel> Metrics { get; set; } = new();
        public string[] WatchedServices { get; set; } = Array.Empty<string>();
    }
}
