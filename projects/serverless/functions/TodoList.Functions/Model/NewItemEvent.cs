namespace ToDoList.Functions;

public class NewItemEvent
{
    public string Subject { get; set; }

    public ToDoItem Item { get; set; }
}