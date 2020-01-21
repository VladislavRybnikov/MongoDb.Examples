using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDb.Examples
{

    public interface IRepository<T> where T : class
    {
        void Insert(T value);
    }

    public interface IMongoDbContext
    {
        void SaveChanges();
        Task SaveChangesAsync();
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

    public class MongoDbSet<T> : IRepository<T>, IMongoQueryable<T> where T : class
    {
        private readonly Lazy<IMongoDatabase> _db;
        private readonly Lazy<IMongoCollection<T>> _collection;
        private readonly Lazy<IMongoQueryable<T>> _queryable;

        public MongoDbSet(Lazy<IMongoDatabase> db)
        {
            var t = typeof(T);
            _db = db;
            _collection = new Lazy<IMongoCollection<T>>(() => _db.Value.GetCollection<T>(t.Name + "s"));
            _queryable = new Lazy<IMongoQueryable<T>>(() => _collection.Value.AsQueryable());
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

        public Type ElementType => _queryable.Value.ElementType;
        public Expression Expression => _queryable.Value.Expression;
        public IQueryProvider Provider => _queryable.Value.Provider;
        public QueryableExecutionModel GetExecutionModel() => _queryable.Value.GetExecutionModel();

        public IAsyncCursor<T> ToCursor(CancellationToken cancellationToken = new CancellationToken()) =>
            _queryable.Value.ToCursor();

        public Task<IAsyncCursor<T>> ToCursorAsync(CancellationToken cancellationToken = new CancellationToken()) =>
            _queryable.Value.ToCursorAsync();
    }
}
