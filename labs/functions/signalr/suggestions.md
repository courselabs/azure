# Lab Suggestions

You can move attribute settings to configuration using a special syntax: `%VariableName%`. So the timer schedule could be defined like this instead:

```
    [FunctionName("broadcast")]
    public static async Task Run(
        [TimerTrigger("%BroadcastTimerSchedule%")] TimerInfo myTimer,
```

And then you can set different values for the schedule in the local settings JSON file and in the Function App appsettings.