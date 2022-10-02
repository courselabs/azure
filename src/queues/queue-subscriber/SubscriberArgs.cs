using PowerArgs;

namespace QueueSubscriber
{
    public class SubscriberArgs
    {
        [ArgRequired(PromptIfMissing = true)]
        [ArgShortcut("cs")]
        public string ConnectionString { get; set; }

        [ArgDefaultValue("echo")]
        [ArgShortcut("q")]
        public string Queue { get; set; }

        [ArgDefaultValue(true)]
        [ArgShortcut("ack")]
        public bool AcknowledgeMessages { get; set; }
    }
}
