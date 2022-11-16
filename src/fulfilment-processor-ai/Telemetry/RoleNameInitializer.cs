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
    }
}