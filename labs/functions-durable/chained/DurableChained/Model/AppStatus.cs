using System;

namespace DurableChained;

public class AppStatus
{
    public string Component { get; set; }
    public string Version { get; set; }
    public DateTime TimestampUtc { get; set; }
    public int StatusCode { get; set; }
    public string StatusMessage { get; set; }
}