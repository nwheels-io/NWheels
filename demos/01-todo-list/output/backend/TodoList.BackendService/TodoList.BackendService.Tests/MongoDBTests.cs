using System;
using MongoDB.Driver;
using NUnit.Framework;

namespace TodoList.BackendService.Tests
{
    [TestFixture]
    public class MongoDBTests
    {
        public class TodoItem
        {
            public TodoItem(int id)
            {
                this.Id = id;
            }
            
            public int Id;
            public string Description;
            public bool Done;
        }

        private IMongoDatabase ConnectToDatbase()
        {
            var credential = MongoCredential.CreateCredential("admin", "root", "example");
            var client = new MongoClient(new MongoClientSettings() {
                Server = new MongoServerAddress("localhost"),
                Credential = credential
            });
            var database = client.GetDatabase("todo_list");

            return database;
        }

        private IMongoCollection<TodoItem> GetTodoItemsCollection(IMongoDatabase db)
        {
            return db.GetCollection<TodoItem>("todo_items");
        }

        [Test]
        public void TryConnect()
        {
            var db = ConnectToDatbase();
            Assert.That(db, Is.Not.Null);
        }

        [Test]
        public void TryRead()
        {
            var db = ConnectToDatbase();
            var collection = GetTodoItemsCollection(db);
            var items = collection.Find(FilterDefinition<TodoItem>.Empty).ToList();

            foreach (var item in items)
            {
                Console.WriteLine($"[{item.Id}] {item.Description}, done={item.Done}");
            }
        }

        [Test]
        public void TryWrite()
        {
            var db = ConnectToDatbase();
            var collection = GetTodoItemsCollection(db);

            var item1 = new TodoItem(1) {
                Description = "AAA",
                Done = true
            };

            var item2 = new TodoItem(2) {
                Description = "BBB",
                Done = false
            };

            collection.InsertMany(new[] {item1, item2});
        }
    }
}