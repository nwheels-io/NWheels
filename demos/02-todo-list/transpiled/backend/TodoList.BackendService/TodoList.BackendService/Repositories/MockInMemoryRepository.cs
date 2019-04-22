using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.BackendService.Domain;

namespace TodoList.BackendService.Repositories
{
    public class MockInMemoryRepository : ITodoItemRepository
    {
        private readonly List<TodoItem> _exampleData = new List<TodoItem> {
            new TodoItem {Id = 111, Description = "First", Done = false},
            new TodoItem {Id = 112, Description = "Second", Done = true},
            new TodoItem {Id = 113, Description = "Third", Done = false},
        };

        private int _nextId = 114; 
        
        public async Task<TodoItem> GetById(int id)
        {
            return _exampleData.FirstOrDefault(item => item.Id == id);
        }

        public async Task<IEnumerable<TodoItem>> GetByQuery(
            QueryFilter<TodoItem> where = null, 
            QueryFilter<TodoItem> orderBy = null)
        {
            var query = _exampleData.AsQueryable();

            if (where != null)
            {
                query = where(query);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return query.ToArray();
        }

        public async Task<TodoItem> Create(string description, bool done)
        {
            var newItem = new TodoItem {
                Id = _nextId++,
                Description = description,
                Done = done
            };
            
            _exampleData.Add(newItem);
            return newItem;
        }

        public async Task Update(TodoItemPatch patch)
        {
            var item = _exampleData.First(x => x.Id == patch.Id);
            
            if (patch.Description != null)
            {
                item.Description = patch.Description;
            }

            if (patch.Done.HasValue)
            {
                item.Done = patch.Done.Value;
            }
        }

        public async Task Delete(IEnumerable<int> ids)
        {
            var idSet = new HashSet<int>(ids);
            var index = _exampleData.FindIndex(x => idSet.Contains(x.Id));
            _exampleData.RemoveAt(index); 
        }
    }
}
