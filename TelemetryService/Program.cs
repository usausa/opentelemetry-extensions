using System.Runtime.InteropServices;

using OpenTelemetry.Metrics;
using OpenTelemetry;

#if WINDOWS_TELEMETRY
using OpenTelemetryExtension.Instrumentation.DiskInfo;
#endif
#if WINDOWS_TELEMETRY
using OpenTelemetryExtension.Instrumentation.HardwareMonitor;
#endif
#if WINDOWS_TELEMETRY
using OpenTelemetryExtension.Instrumentation.PerformanceCounter;
#endif
using OpenTelemetryExtension.Instrumentation.SensorOmron;
#if WINDOWS_TELEMETRY
using OpenTelemetryExtension.Instrumentation.SwitchBot.Windows;
#endif
using OpenTelemetryExtension.Instrumentation.WFWattch2;

using Serilog;

using TelemetryService;
using TelemetryService.Settings;
using TelemetryService.Metrics;

// Builder
Directory.SetCurrentDirectory(AppContext.BaseDirectory);
var builder = Host.CreateApplicationBuilder(args);
var useOtlpExporter = !String.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

// Setting
var setting = builder.Configuration.GetSection("Telemetry").Get<TelemetrySetting>()!;

// Service
builder.Services
    .AddWindowsService()
    .AddSystemd();

// Logging
builder.Logging.ClearProviders();
builder.Services.AddSerilog(options =>
{
    options.ReadFrom.Configuration(builder.Configuration);
}, writeToProviders: useOtlpExporter);

// Metrics
builder.Services
    .AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        var host = setting.Host ?? Environment.MachineName;

        if (setting.EnableApplicationMetrics)
        {
            metrics.AddApplicationInstrumentation();
        }
#if WINDOWS_TELEMETRY
        if (setting.EnableHardwareMetrics)
        {
            setting.HardwareMonitor.Host = String.IsNullOrWhiteSpace(setting.HardwareMonitor.Host) ? host : setting.HardwareMonitor.Host;
            metrics.AddHardwareMonitorInstrumentation(setting.HardwareMonitor);
        }
#endif
#if WINDOWS_TELEMETRY
        if (setting.EnableDiskInfoMetrics)
        {
            setting.DiskInfo.Host = String.IsNullOrWhiteSpace(setting.DiskInfo.Host) ? host : setting.DiskInfo.Host;
            metrics.AddDiskInfoInstrumentation(setting.DiskInfo);
        }
#endif
#if WINDOWS_TELEMETRY
        if (setting.EnablePerformanceCounterMetrics)
        {
            setting.PerformanceCounter.Host = String.IsNullOrWhiteSpace(setting.PerformanceCounter.Host) ? host : setting.PerformanceCounter.Host;
            metrics.AddPerformanceCounterInstrumentation(setting.PerformanceCounter);
        }
#endif
        if (setting.EnableSensorOmronMetrics)
        {
            metrics.AddSensorOmronInstrumentation(setting.SensorOmron);
        }
        if (setting.EnableWFWattch2Metrics)
        {
            metrics.AddWFWattch2Instrumentation(setting.WFWattch2);
        }
#if WINDOWS_TELEMETRY
        if (setting.EnableSwitchBotMetrics)
        {
            metrics.AddSwitchBotInstrumentation(setting.SwitchBot);
        }
#endif

        metrics.AddPrometheusHttpListener(options =>
        {
            options.UriPrefixes = setting.EndPoints;
        });
        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }
    });

// Build
var host = builder.Build();

// Startup
var log = host.Services.GetRequiredService<ILogger<Program>>();
log.InfoServiceStart();
log.InfoServiceSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
log.InfoServiceSettingsEnvironment(typeof(Program).Assembly.GetName().Version, Environment.CurrentDirectory);

host.Run();
