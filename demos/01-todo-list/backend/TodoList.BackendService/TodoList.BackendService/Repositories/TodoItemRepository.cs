using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<TodoItem>> GetByQuery(
            QueryFilter<TodoItem> where = null, 
            QueryFilter<TodoItem> orderBy = null)
        {
            IQueryable<TodoItem> query = GetTodoItemCollection().AsQueryable();

            if (where != null)
            {
                query = where(query);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var mongoQuery = (IMongoQueryable<TodoItem>)query;
            var buffer = await mongoQuery.ToListAsync();

            return buffer;
        }

        public async Task<TodoItem> Create(string description, bool done)
        {
            var id = await _db.TakeNextSequenceNumber(id: "todo_items");
            
            var item = new TodoItem {
                Id = id,
                Description = description,
                Done = done
            };

            await GetTodoItemCollection().InsertOneAsync(item);

            return item;
        }

        public Task Update(TodoItemPatch patch)
        {
            var builder = Builders<TodoItem>.Update;
            var updates = new List<UpdateDefinition<TodoItem>>();

            if (patch.Description != null)
            {
                updates.Add(builder.Set(x => x.Description, patch.Description));
            }

            if (patch.Done.HasValue)
            {
                updates.Add(builder.Set(x => x.Done, patch.Done.Value));
            }

            return GetTodoItemCollection()
                .UpdateOneAsync(ById<TodoItem, int>(patch.Id), builder.Combine(updates));
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
