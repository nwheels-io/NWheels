using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using NWheels.Testing.Entities.Stacks;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Stacks.MongoDb.ComponentTests
{
    [TestFixture, Category("Integration")]
    public class HardCodedImplementationTests
    {
        public const string DatabaseName = "NWheelsTest";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MongoDatabase _database;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            var client = new MongoClient();
            var server = client.GetServer();

            if ( server.DatabaseExists("NWheelsTest") )
            {
                server.DropDatabase("NWheelsTest");
            }

            _database = server.GetDatabase("NWheelsTest");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Test]
        public void HelloWorld()
        {
            var collection = _database.GetCollection<Person>("Person");

            var inserted = new Person() { Id = 1003, Name = "Smith", Age = 31 };

            collection.Insert(inserted);

            var retrieved = collection.AsQueryable().First(p => p.Age > 20);

            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved, Is.Not.SameAs(inserted));
            Assert.That(retrieved.Id, Is.EqualTo(1003));
            Assert.That(retrieved.Name, Is.EqualTo("Smith"));
            Assert.That(retrieved.Age, Is.EqualTo(31));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void BasicCrudOperations()
        {
            CrudOperations.Repository1.ExecuteBasic(repoFactory: InitializeHardCodedDataRepository);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IR1.IOnlineStoreRepository InitializeHardCodedDataRepository()
        {
            return new HardCodedImplementations.DataRepositoryObject_OnlineStoreRepository();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Person
        {
            [BsonId]
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
