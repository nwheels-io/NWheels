namespace TodoList.BackendService.Repositories
{
    public class TodoItemPatch
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool? Done { get; set; }
    }
}
