using PowerArgs;

namespace Publisher
{
    public class PublisherArgs
    {
        [ArgRequired(PromptIfMissing = true)]
        [ArgShortcut("cs")]
        public string ConnectionString { get; set; }

        [ArgDefaultValue("echo")]
        [ArgShortcut("q")]
        public string Queue { get; set; }

        [ArgShortcut("t")]
        public string Topic { get; set; }

        [ArgDefaultValue(5)]
        [ArgShortcut("b")]
        public int BatchSize { get; set; }

        [ArgDefaultValue(20)]
        [ArgShortcut("s")]
        public int SleepSeconds { get; set; }
    }
}
