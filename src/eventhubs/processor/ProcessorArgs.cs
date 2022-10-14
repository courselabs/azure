using PowerArgs;

namespace Processor;

public class ProcessorArgs
{
    [ArgRequired(PromptIfMissing = true)]
    [ArgShortcut("cs")]
    public string ConnectionString { get; set; }

    [ArgRequired(PromptIfMissing = true)]
    [ArgShortcut("scs")]
    public string StorageConnectionString { get; set; }

    [ArgDefaultValue("checkpoints")]
    [ArgShortcut("sc")]
    public string StorageContainer { get; set; }

    [ArgDefaultValue("devicelogs")]
    [ArgShortcut("e")]
    public string EventHub { get; set; }

    [ArgDefaultValue("processing")]
    [ArgShortcut("g")]
    public string ConsumerGroup { get; set; }

    [ArgDefaultValue(100)]
    [ArgShortcut("su")]
    public int StatusUpdateFrequency { get; set; }
}