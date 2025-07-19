

using Serilog;
using TelemetriaServidor;
using TelemetriaServidor.Interface;
using TelemetriaServidor.Model;
using TelemetriaServidor.Service;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// obtém o IP da máquina **uma única vez**
var localIp = NetworkInfo.GetLocalIPv4();

// Inicializa o Log estático do Serilog antes de construir o Host
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)           // lê nível e sinks de appsettings.json
    .Enrich.FromLogContext()                         // adiciona propriedades de contexto automaticamente
                                                     // adiciona a propriedade MachineIP em TODOS os eventos
    .Enrich.WithProperty("MachineIP", localIp)
    .CreateLogger();

try
{
    var builder = Host.CreateDefaultBuilder(args)
        .UseSerilog()                               // plug Serilog no ASP.NET / Generic Host
        .ConfigureServices((ctx, services) =>
        {
            services.Configure<TelemetryOptions>(ctx.Configuration.GetSection("Telemetry"));
            services.AddSingleton<IMetricsProvider, MetricsProvider>();
            services.AddHostedService<Worker>();
        });

    await builder.Build().RunAsync();
}
finally
{
    Log.CloseAndFlush();
}