using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TodoList.BackendService.Domain;

namespace TodoList.BackendService.Repositories
{
    public class TodoItemRepository : ITodoItemRepository
    {
        private readonly IMongoDatabase _db;
        
        public TodoItemRepository(MongoDBConfig config)
        {
            _db = MongoDBHelpers.Connect(config);
        }
        
        public Task<TodoItem> GetById(int id)
        {
            return GetTodoItemCollection()
                .Find(ById<TodoItem, int>(id))
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TodoItem>> GetByQuery(string description, bool? done)
        {
            var query = GetTodoItemCollection().AsQueryable();

            if (description != null)
            {
                query = query.Where(x => x.Description.Contains(description));
            }

            if (done.HasValue)
            {
                query = query.Where(x => x.Done == done);
            }

            var buffer = await query.ToListAsync();
            return buffer;
        }

        public Task Update(TodoItem item)
        {
            return GetTodoItemCollection()
                .ReplaceOneAsync(ById<TodoItem, int>(item.Id), item);
        }

        public Task Delete(IEnumerable<int> ids)
        {
            return GetTodoItemCollection().DeleteManyAsync(ByIds<TodoItem, int>(ids));
        }

        private IMongoCollection<TodoItem> GetTodoItemCollection()
        {
            return _db.GetCollection<TodoItem>("todo_items");
        }

        private FilterDefinition<TDocument> ById<TDocument, TId>(TId id)
        {
            return Builders<TDocument>.Filter.Eq("_id", id);
        }

        private FilterDefinition<TDocument> ByIds<TDocument, TId>(IEnumerable<TId> ids)
        {
            return Builders<TDocument>.Filter.In("_id", ids);
        }
    }
}
