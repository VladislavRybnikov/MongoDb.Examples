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

            var res = from user in db.Users
                where user.Name.Length > 5
                select user.Name;

            var res2 = 
                from user in db.Users.FromCollection("users2")
                where user.Name.Length > 5
                select user.Name;

            res.ToList().ForEach(Console.WriteLine);
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
