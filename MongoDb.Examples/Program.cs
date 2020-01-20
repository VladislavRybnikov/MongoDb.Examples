using System;
using System.Linq;
using System.Reflection;
using MongoDB.Driver;

namespace MongoDb.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var db = new MyMongoDbContext();

            db.Users.ToList().ForEach(x => Console.WriteLine(x.Name));
        }
    }

    public class MyMongoDbContext : MongoDbContext
    {
        public MyMongoDbContext() : base("Test")
        {
        }

        public MongoDbSet<User> Users { get; set; }
    }

    public class User
    {
        public string Name { get; set; }
    }

}
