using PowerArgs;

namespace Consumer
{
    public class ConsumerArgs
    {
        [ArgRequired(PromptIfMissing = true)]
        [ArgShortcut("cs")]
        public string ConnectionString { get; set; }

        [ArgDefaultValue("devicelogs")]
        [ArgShortcut("e")]
        public string EventHub { get; set; }

        [ArgDefaultValue("$Default")]
        [ArgShortcut("g")]
        public string ConsumerGroup { get; set; }
    }
}
