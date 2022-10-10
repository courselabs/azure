using PowerArgs;

namespace Subscriber
{
    public class SubscriberArgs
    {
        [ArgRequired(PromptIfMissing = true)]
        [ArgShortcut("cs")]
        public string ConnectionString { get; set; }

        [ArgDefaultValue("echo")]
        [ArgShortcut("q")]
        public string Queue { get; set; }

        [ArgShortcut("t")]
        public string Topic { get; set; }

        [ArgShortcut("s")]
        public string Subscription { get; set; }

        [ArgDefaultValue(true)]
        [ArgShortcut("ack")]
        public bool AcknowledgeMessages { get; set; }
    }
}
