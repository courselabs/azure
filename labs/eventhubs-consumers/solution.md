# Lab Solution

Open the `checkpoints` container in your storage account - this is where the processors are recording state:

- you'll see a folder structure with the event hub & consumer group name
- e.g. `<eh-name>.servicebus.windows.net/devicelogs/processing`
- there are two folders in there - `checkpoint` and `ownership`
- each has 5 files - numbered 0 to 4 for the partitions
- the files are empty; offsets and owner IDs are stored as metadata

This is how the processor allocates partitions, by recording an owner ID in the metadata. Each owner records the offset it has processed up to - for that partition - in the checkpoint file.

Stop a processor and one of the others will take ownership of its partition(s), using the offset to pick up from the last message. It will take a few minutes, because the library waits to make sure the partition really isn't being processed before it gets reallocated, so there are not two processors working it.