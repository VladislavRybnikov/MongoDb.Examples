using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDb.Examples
{

    public interface IRepository<T> where T : class
    {
        void Insert(T value);
    }

    public abstract class MongoDbContext
    {
        private readonly Lazy<IMongoDatabase> _db;

        protected MongoDbContext(string dbName)
        {
            var client = new MongoClient();
            _db = new Lazy<IMongoDatabase>(() => client.GetDatabase(dbName));

            var props = GetType().GetProperties();
            props.Where(p => p.PropertyType.Name == "MongoDbSet`1")
                .ToList().ForEach(SetProp);
        }

        private void SetProp(PropertyInfo p)
        {
            var inst = Activator.CreateInstance(p.PropertyType, _db);

            p.SetValue(this, inst);
        }
    }

    public class MongoDbSet<T> : IRepository<T>, IEnumerable<T> where T : class
    {
        private readonly Lazy<IMongoDatabase> _db;
        private readonly Lazy<IMongoCollection<T>> _collection;

        public MongoDbSet(Lazy<IMongoDatabase> db)
        {
            var t = typeof(T);
            _db = db;
            _collection = new Lazy<IMongoCollection<T>>(() => _db.Value.GetCollection<T>(t.Name + "s"));
        }

        public void Insert(T value)
        {
            _collection.Value.InsertOne(value);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.Value.Find(new BsonDocument(), new FindOptions
            {
                ShowRecordId = false,
            }).ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
