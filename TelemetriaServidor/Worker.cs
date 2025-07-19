using System.ServiceProcess;
using TelemetriaServidor.Interface;

namespace TelemetriaServidor
{
    
    public sealed class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _log;
        private readonly TimeSpan _interval;
        private readonly IMetricsProvider _metricsProvider;
        private readonly string[] _watchedSvcs;

        /// <summary>
        /// Coleta e registra métricas do sistema, incluindo memória, disco e contadores personalizados.
        /// </summary>
        /// <param name="log">Instância de ILogger para registrar informações e avisos.</param>
        /// <param name="cfg">Configuração do serviço, utilizada para obter intervalos e serviços monitorados.</param>
        /// <param name="metricsProvider">Provedor de métricas que expõe contadores de desempenho.</param>
        /// <returns>Não retorna valor; construtor inicializa o serviço Worker.</returns>
        public Worker(ILogger<Worker> log,
                      IConfiguration cfg,
                      IMetricsProvider metricsProvider)
        {
            _log = log;
            _metricsProvider = metricsProvider;
            _interval = TimeSpan.FromSeconds(cfg.GetValue("Telemetry:IntervalSeconds", 30));
            _watchedSvcs = cfg.GetSection("Telemetry:WatchedServices")
                             .Get<string[]>() ?? Array.Empty<string>();

            // Warm-up
            foreach (var (pc, _) in _metricsProvider.Counters.Values)
                _ = pc.NextValue();
        }

        /// <summary>
        /// Executa o serviço em segundo plano, coletando e registrando métricas periodicamente.
        /// </summary>
        /// <param name="stoppingToken">Token de cancelamento para interromper a execução assíncrona.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona de execução do serviço.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                LogMetrics();
                CheckServices();
                await Task.Delay(_interval, stoppingToken);
            }
        }

        /// <summary>
        /// Coleta e registra métricas do sistema, incluindo memória, disco e contadores personalizados.
        /// </summary>
        /// <remarks>
        /// Este método coleta métricas de desempenho do sistema, incluindo memória disponível, percentual de memória usada,
        /// métricas derivadas de memória, métricas de disco por unidade e registra todas as informações utilizando o logger.
        /// </remarks>
        /// <returns>Não retorna valor; apenas realiza o registro das métricas coletadas.</returns>
        private void LogMetrics()
        {
            var timestamp = DateTime.UtcNow;
            double? availableMb = null, percentUsed = null;

            // 1) métrica “fixas” via PerformanceCounter
            foreach (var kv in _metricsProvider.Counters)
            {
                var name = kv.Key;
                var (pc, ty) = kv.Value;
                var value = pc.NextValue();

                // guarda pra Memória derivada
                if (name == "MemAvailableMb") availableMb = value;
                if (name == "MemPercentUsed") percentUsed = value;

                _log.LogInformation(
                    "Metric {MetricName} {Value:0.##} Type {MetricType} Timestamp {Timestamp:O}",
                    name, value, ty, timestamp);
            }

            // 2) métricas derivadas de Memória (total, usado, % usado)…
            if (MachineInfo.TotalPhysicalMemoryMb > 0 && availableMb.HasValue)
            {
                var total = MachineInfo.TotalPhysicalMemoryMb;

                // extrai só os 2 dígitos mais significativos (→ 32)
                int memorariaEmMb = GetLeadingDigits(total, 2);

                var usedMb = total - availableMb.Value;
                var usedPct = usedMb / total * 100;

                LogMemDerived("MemTotalMb", memorariaEmMb, timestamp);
                LogMemDerived("MemUsedMb", usedMb, timestamp);
                LogMemDerived("MemUsedPct", usedPct, timestamp);
            }

            // 3) **NOVO** – métricas de disco por letra de drive
            foreach (var drive in DriveInfo.GetDrives()
                                           .Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
            {
                var letter = drive.Name.TrimEnd('\\');            // ex: "C:"
                var totalGb = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                var freeGb = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
                var usedGb = totalGb - freeGb;
                var usedPct = usedGb / totalGb * 100;

                // helper para logar 4 métricas de uma vez:
                LogDiskMetric($"{letter}_TotalGb", totalGb, timestamp);
                LogDiskMetric($"{letter}_FreeGb", freeGb, timestamp);
                LogDiskMetric($"{letter}_UsedGb", usedGb, timestamp);
                LogDiskMetric($"{letter}_UsedPct", usedPct, timestamp);
            }
        }

        /// <summary>
        /// <summary>
        /// Registra uma métrica derivada de memória no log.
        /// </summary>
        /// <param name="name">Nome da métrica de memória derivada.</param>
        /// <param name="value">Valor da métrica a ser registrada.</param>
        /// <param name="ts">Timestamp da coleta da métrica.</param>
        /// <returns>Não retorna valor; apenas realiza o registro da métrica no log.</returns>
        /// </summary>
        private void LogMemDerived(string name, double value, DateTime ts) =>
    _log.LogInformation("Metric {MetricName} {Value:0.##} Type {MetricType} Timestamp {Timestamp:O}",
                        name, value, "Memória", ts);

        /// <summary>
        /// <summary>
        /// Registra uma métrica de disco no log.
        /// </summary>
        /// <param name="name">Nome da métrica de disco.</param>
        /// <param name="value">Valor da métrica a ser registrada.</param>
        /// <param name="ts">Timestamp da coleta da métrica.</param>
        /// <returns>Não retorna valor; apenas realiza o registro da métrica no log.</returns>
        /// </summary>
        private void LogDiskMetric(string name, double value, DateTime ts) =>
            _log.LogInformation("Metric {MetricName} {Value:0.##} Type {MetricType} Timestamp {Timestamp:O}",
                                name, value, "Disco", ts);

      
        /// <sumary>
        /// Verifica o status dos serviços monitorados e registra um aviso caso algum não esteja em execução.
        /// </summary>
        /// <remarks>
        /// Este método percorre a lista de serviços definidos em <c>_watchedSvcs</c>, verifica seu status utilizando <see cref="ServiceController"/>,
        /// e registra um aviso no log se o serviço não estiver com o status <see cref="ServiceControllerStatus.Running"/>.
        /// </remarks>
        /// <returns>Não retorna valor; apenas realiza a verificação e registro de avisos no log.</returns>
      
        private void CheckServices()
        {
            foreach (var svc in _watchedSvcs)
            {
                try
                {
                    using var sc = new ServiceController(svc);
                    if (sc.Status != ServiceControllerStatus.Running)
                        _log.LogWarning("Service {Svc} status {Status}", svc, sc.Status);
                }
                catch { /* ignore */ }
            }
        }

        /// <summary>
        /// Retorna os <paramref name="numDigits"/> dígitos mais significativos de <paramref name="value"/>.
        /// Ex.: value=32500.207, numDigits=2 → retorna 32
        /// </summary>
        private static int GetLeadingDigits(double value, int numDigits = 2)
        {
            // Trunca para descartar parte fracionária
            var n = (long)Math.Truncate(value);
            if (n == 0) return 0;

            // Quantos dígitos tem n?
            int length = (int)Math.Floor(Math.Log10(n)) + 1;

            // Se tiver mais dígitos que queremos, divida pela potência apropriada
            long divisor = length > numDigits
                ? (long)Math.Pow(10, length - numDigits)
                : 1L;

            return (int)(n / divisor);
        }

    }

}
