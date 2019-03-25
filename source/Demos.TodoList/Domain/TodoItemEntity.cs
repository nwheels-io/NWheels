using NWheels.Domain.Model;

namespace Demos.TodoList.Domain
{
    public class TodoItemEntity
    {
        public readonly int Id;
        
        [MaxLength(100)]
        public string Title;

        public bool Done;
    }
}
