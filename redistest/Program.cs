using System;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace redistest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            InitializeConfiguration();

            // Connection refers to a property that returns a ConnectionMultiplexer
            // as shown in the previous example.
            IDatabase cache = lazyConnection.Value.GetDatabase();

            // Perform cache operations using the cache object...

            // Simple PING command
            string cacheCommand = "PING";
            Console.WriteLine("\nCache command  : " + cacheCommand);
            Console.WriteLine("Cache response : " + cache.Execute(cacheCommand).ToString());

            // Simple get and put of integral data types into the cache
            cacheCommand = "GET Message";
            Console.WriteLine("\nCache command  : " + cacheCommand + " or StringGet()");
            Console.WriteLine("Cache response : " + cache.StringGet("Message").ToString());

            cacheCommand = "SET Message \"Hello! The cache is working from a .NET Core console app!\"";
            Console.WriteLine("\nCache command  : " + cacheCommand + " or StringSet()");
            Console.WriteLine("Cache response : " + cache.StringSet("Message", "Hello! The cache is working from a .NET Core console app!").ToString());

            // Demonstrate "SET Message" executed as expected...
            cacheCommand = "GET Message";
            Console.WriteLine("\nCache command  : " + cacheCommand + " or StringGet()");
            Console.WriteLine("Cache response : " + cache.StringGet("Message").ToString());

            // Get the client list, useful to see if connection list is growing...
            cacheCommand = "CLIENT LIST";
            Console.WriteLine("\nCache command  : " + cacheCommand);
            Console.WriteLine("Cache response : \n" + cache.Execute("CLIENT", "LIST").ToString().Replace("id=", "id="));

            // [sk]:

            // Get the client list, useful to see if connection list is growing...
            cacheCommand = "Set List";
            Console.WriteLine("\nCache command  : " + cacheCommand);
            var listKey = "list1";
            cache.KeyDelete(listKey);
            cache.ListRightPush("list1", "a");
            cache.ListRightPush("list1", "b");
            cache.ListRightPush("list1", "c");
            cache.ListRightPush("list1", "d");
            Console.WriteLine("list1 elements: " + string.Concat(cache.ListRange(listKey)));

            var hashKey = "hash1";
            cache.KeyDelete(hashKey);
            HashEntry[] redisBookHash = {
                new HashEntry("title", "Title A"),
                new HashEntry("year", 2016),
                new HashEntry("author", "Author A")
            };

            cache.HashSet(hashKey, redisBookHash);

            if (cache.HashExists(hashKey, "year"))
            {
                var year = cache.HashGet(hashKey, "year"); //year is 2016
            }

            //get all the items
            var allHash = cache.HashGetAll(hashKey);
            foreach (var item in allHash)
            {
                Console.WriteLine(string.Format("key : {0}, value : {1}", item.Name, item.Value));
            }

            cache.HashSet(hashKey, "author", "Author BBB", When.Always);

            //get all the items
            allHash = cache.HashGetAll(hashKey);
            foreach (var item in allHash)
            {
                Console.WriteLine(string.Format("key : {0}, value : {1}", item.Name, item.Value));
            }


            lazyConnection.Value.Dispose();
        }

        private static IConfigurationRoot Configuration { get; set; }
        const string SecretName = "RedisConnection";

        private static void InitializeConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<Program>();

            Configuration = builder.Build();
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = Configuration[SecretName];
            return ConnectionMultiplexer.Connect(cacheConnection);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}
