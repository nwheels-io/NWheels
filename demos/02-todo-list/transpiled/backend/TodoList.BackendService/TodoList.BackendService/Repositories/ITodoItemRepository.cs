using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.BackendService.Domain;

namespace TodoList.BackendService.Repositories
{
    public delegate IQueryable<T> QueryFilter<T>(IQueryable<T> source);

    public interface ITodoItemRepository
    {
        Task<TodoItem> GetById(int id);
        Task<IEnumerable<TodoItem>> GetByQuery(
            QueryFilter<TodoItem> where = null, 
            QueryFilter<TodoItem> orderBy = null);
        Task<TodoItem> Create(string description, bool done);
        Task Update(TodoItemPatch patch);
        Task Delete(IEnumerable<int> ids);
    }
}