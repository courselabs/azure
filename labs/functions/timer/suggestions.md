# Lab Suggestions

The guiding rule is one function per piece of logic. If you had multiple components to check on you might want a separate function for each so you could use different schedules or different output container for the blobs.

The output path adds a timestamp to the blob name. If you needed to find the latest status you'd have to enumerate the container and find the most recent blob. Alternatively you could write to a static blob name - so you have one blob with the latest status, which gets overwritten each time the trigger fires.
