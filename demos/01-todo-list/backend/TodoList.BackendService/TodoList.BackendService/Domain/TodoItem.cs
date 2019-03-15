namespace TodoList.BackendService.Domain
{
    public class TodoItem
    {
        public TodoItem(int id)
        {
            Id = id;
        }

        public int Id;
        public string Description;
        public bool Done;
    }    
}