namespace OpenTelemetryExtension.Instrumentation.SensorOmron;

public sealed class DeviceEntry
{
    public string Port { get; set; } = default!;

    public string Name { get; set; } = default!;
}

#pragma warning disable CA1819
public sealed class SensorOmronOptions
{
    public int Interval { get; set; } = 5000;

    public DeviceEntry[] Device { get; set; } = default!;
}
#pragma warning restore CA1819
