using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace FulfilmentProcessor;

public class RoleNameInitializer : ITelemetryInitializer
{
    private readonly string _roleName;

    public RoleNameInitializer(string roleName)
    {
        _roleName = roleName;
    }

    void ITelemetryInitializer.Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = _roleName;
        telemetry.Context.Component.Version = typeof(RoleNameInitializer).Assembly.GetName().Version.ToString();

        Console.WriteLine($"** Initializer set RoleName: {telemetry.Context.Cloud.RoleName}; Version: {telemetry.Context.Component.Version}");
    }
}