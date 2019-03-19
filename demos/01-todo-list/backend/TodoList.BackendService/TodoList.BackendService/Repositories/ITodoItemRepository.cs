using System.Collections.Generic;
using System.Threading.Tasks;
using TodoList.BackendService.Domain;

namespace TodoList.BackendService.Repositories
{
    public interface ITodoItemRepository
    {
        Task<TodoItem> GetById(int id);
        Task<IEnumerable<TodoItem>> GetByQuery(int? id, string description, bool? done);
        Task<TodoItem> Create(string description, bool done);
        Task Update(TodoItemPatch patch);
        Task Delete(IEnumerable<int> ids);
    }
}