# Lab Hints

When the processors start they pick up one partition each to begin with, then they'll pick up more when they see there are partitions with no active processors.

It will take a while for some of the experiments because the processors wait to make sure they don't duplicate work - you might think this is pretty slow for a service that's meant to process billions of events. But it's only the partition allocation that takes time, as soon as a processor has its partitions it reads through events constantly.

> Need more? Here's the [solution](solution.md).