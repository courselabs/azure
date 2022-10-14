
in the `checkpoints` container

- folder structure e.g. labseventhubsconsumerses.servicebus.windows.net/devicelogs/processing - event hub & consumer group name
- there are two folders - checkpoint and ownsership
- each has 5 files - numbered 0 to 4 for the partitions
- the files are empty; offsets and owner IDs are stored as metadata

Stop a processor and another one will take owvership of its partition(s), using the offset to pick up from the last message.