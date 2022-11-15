# Lab Suggestions

Yes you can disable internally-triggered functions like the activity triggers. The other functions still run, so the timer fires and creates the orchestrator. The orchestrator tries to run the activity functions, but what happens next may not be what you expect.

These are durable functions which are able to keep running. Open the orchestrator function in the Portal and under the _Monitor_ tab you won't see any errors. There's an _Orchestrations_ page which will show you what's happening.

When an activity can't be called, the orchestration instance stays in the _Running_ state, with the disabled activity in the status _Scheduled_ and the orchestration instance in the status _Awaited_.

Enable the activity again and the running orchestrations will complete.
