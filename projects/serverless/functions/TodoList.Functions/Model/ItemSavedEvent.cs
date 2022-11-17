using System;

namespace ToDoList.Functions;

public class ItemSavedEvent
{
    public string Subject { get; set; }

    public ToDoItem Item { get; set; }

    public DateTime SavedAt { get; set; }
}
