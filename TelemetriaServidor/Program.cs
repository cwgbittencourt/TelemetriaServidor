using Serilog;
using TelemetriaServidor;
using TelemetriaServidor.Interface;
using TelemetriaServidor.Model;
using TelemetriaServidor.Service;

// 1. Carrega appsettings.json (com reload on change)
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// 2. Recupera IP local uma única vez
var localIp = NetworkInfo.GetLocalIPv4();

// 3. Inicializa Serilog com MachineIP
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("MachineIP", localIp)
    .CreateLogger();

try
{
    // 4. Cria e configura o Host para rodar como Windows Service
    var host = Host.CreateDefaultBuilder(args)
        .UseWindowsService()         // registra Start/Stop no SCM sem time-out
        .UseSerilog()                // plug Serilog no Generic Host
        .ConfigureServices((ctx, services) =>
        {
            // seu options pattern
            services.Configure<TelemetryOptions>(
                ctx.Configuration.GetSection("Telemetry"));

            services.AddSingleton<IMetricsProvider, MetricsProvider>();
            services.AddHostedService<Worker>();
        })
        .Build();

    // 5. Inicia o loop de BackgroundService (Worker) — SCM já liberado
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "O serviço terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
