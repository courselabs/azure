using PowerArgs;
using StackExchange.Redis;

namespace Pi;

class Program
{
    private static Arguments Arguments;
    private static ConnectionMultiplexer Redis;

    static void Main(string[] args)
    {
        Arguments = Args.Parse<Arguments>(args);     
        if (!string.IsNullOrEmpty(Arguments.ConnectionString))
        {   
            Redis = ConnectionMultiplexer.Connect(Arguments.ConnectionString);
        }

        string pi;
        if (Arguments.UseCache && Redis != null)
        {
            var db = Redis.GetDatabase();
            var key=$"pi-{Arguments.DecimalPlaces}";
            pi = db.StringGet("key");
            if (string.IsNullOrEmpty(pi))
            {
                pi = GetPi();
                db.StringSet(key, pi);
            }
        }
        else
        {
            pi = GetPi();
        }

        Console.WriteLine(pi);
    }

    static string GetPi()
    {
        var pi = MachinFormula.Calculate(Arguments.DecimalPlaces).ToString();
        if (Arguments.PublishEvents && Redis != null)
        {
            var subscriber = Redis.GetSubscriber();
            subscriber.Publish("events.pi.computed", $"Calculated Pi to: {Arguments.DecimalPlaces}dp");
        }
        return pi;
    }
}