using PowerArgs;

namespace Pi;

[AllowUnexpectedArgs]
public class Arguments
{
    [ArgDefaultValue(10)]
    [ArgShortcut("dp")]
    [ArgDescription("Decimal places to calculate")]
    public int DecimalPlaces { get; set; }

    [ArgShortcut("cs")]
    [ArgDescription("Redis connection string")]
    public string ConnectionString { get; set; }

    [ArgDefaultValue(false)]
    [ArgShortcut("cache")]
    [ArgDescription("Cache calculation results")]
    public bool UseCache { get; set; }

    [ArgDefaultValue(false)]
    [ArgShortcut("events")]
    [ArgDescription("Publish calculation events")]
    public bool PublishEvents { get; set; }
}
