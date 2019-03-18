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

        public async Task<IEnumerable<TodoItem>> GetByQuery(int? id, string description, bool? done)
        {
            return _exampleData.Where(item =>    
                (!id.HasValue || id.Value == item.Id) &&
                (description == null || item.Description.Contains(description)) &&
                (!done.HasValue || done.Value == item.Done)
            );
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

        public async Task Update(TodoItem item)
        {
            var index = _exampleData.FindIndex(x => x.Id == item.Id);
            _exampleData[index] = item; 
        }

        public async Task Delete(IEnumerable<int> ids)
        {
            var idSet = new HashSet<int>(ids);
            var index = _exampleData.FindIndex(x => idSet.Contains(x.Id));
            _exampleData.RemoveAt(index); 
        }
    }
}
