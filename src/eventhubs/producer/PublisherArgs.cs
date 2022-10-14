using PowerArgs;

namespace Producer;

public class ProducerArgs
{
    [ArgRequired(PromptIfMissing = true)]
    [ArgShortcut("cs")]
    public string ConnectionString { get; set; }

    [ArgDefaultValue("devicelogs")]
    [ArgShortcut("e")]
    public string EventHub { get; set; }

    [ArgDefaultValue(10)]
    [ArgShortcut("p")]
    public int ProducerCount { get; set; }

    [ArgDefaultValue(10)]
    [ArgShortcut("b")]
    public int BatchSize { get; set; }
}