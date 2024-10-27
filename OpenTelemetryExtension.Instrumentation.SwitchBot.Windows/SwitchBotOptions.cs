namespace OpenTelemetryExtension.Instrumentation.SwitchBot.Windows;

public sealed class DeviceEntry
{
    public string Address { get; set; } = default!;

    public string Name { get; set; } = default!;
}

#pragma warning disable CA1819
public sealed class SwitchBotOptions
{
    public int TimeThreshold { get; set; } = 300_000;

    public DeviceEntry[] Device { get; set; } = default!;
}
#pragma warning restore CA1819
