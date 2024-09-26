using Integration.Backend;
using Integration.Service;
using RedLockNet.SERedis.Configuration;
using RedLockNet.SERedis;
using StackExchange.Redis;

namespace Integration;

public abstract class Program
{
    public static void Main(string[] args)
    {
        #region Single Server Scenario
        //var service = new ItemIntegrationService();

        //var tasks = new List<Task>();

        //// first three task
        //tasks.Add(Task.Run(() => service.SaveItemAsync("a")));
        //tasks.Add(Task.Run(() => service.SaveItemAsync("b")));
        //tasks.Add(Task.Run(() => service.SaveItemAsync("c")));


        //Thread.Sleep(500);

        //// trying record same content
        //tasks.Add(Task.Run(() => service.SaveItemAsync("a")));
        //tasks.Add(Task.Run(() => service.SaveItemAsync("b")));
        //tasks.Add(Task.Run(() => service.SaveItemAsync("c")));

        //// wait for all tasks to complete
        //Task.WhenAll(tasks).Wait();

        //// wait 5 second
        //Thread.Sleep(5000);

        //// write to console
        //Console.WriteLine("Everything recorded:");

        //service.GetAllItems().ForEach(item => Console.WriteLine($"Item Content: {item.Content}, Item ID: {item.Id}"));


        //Console.ReadLine();
        #endregion

        //*******************************************************************************

        #region Distributed Scenario solution 1 with redis
        //var redis = ConnectionMultiplexer.Connect("localhost");
        //var db = redis.GetDatabase();
        //var itemService = new DistributedItemService(db);

        //// Record two different contents in parallel
        //var tasks = new List<Task>
        //{
        //    Task.Run(async () => Console.WriteLine(await itemService.SaveItemAsync("Item1"))),
        //    Task.Run(async () => Console.WriteLine(await itemService.SaveItemAsync("Item2"))),
        //    Task.Run(async () => Console.WriteLine(await itemService.SaveItemAsync("Item1"))) // Çakışma durumu
        //};

        //Task.WhenAll(tasks); 
        #endregion

        //*******************************************************************************

        #region Distributed Scenario solution 2 with redis

        var redis = ConnectionMultiplexer.Connect("localhost:6379");
        var redLockFactory = RedLockFactory.Create(new List<RedLockMultiplexer>
        {
            redis
        });

        // Lock 
        using var redLock = redLockFactory.CreateLockAsync("lock-key", TimeSpan.FromSeconds(30)).Result;
        if (redLock.IsAcquired)
        {
            // continue if lock acquired
            IDatabase db = redis.GetDatabase();
            // sample items
            string[] items = { "item11", "item21", "item11", "item31", "item21" };

            foreach (var item in items)
            {
                // check if item already exists
                bool isAdded = AddItemIfNotExists(db, item);

                if (isAdded)
                {
                    Console.WriteLine($"unique content: {item}");
                }
                else
                {
                    Console.WriteLine($"Duplicate content: {item}");
                }
            }
        }
        else
        {
            // stop if the lock is not obtained
            Console.WriteLine("Could not acquire lock.");
        }
        #endregion
    }

    private static bool AddItemIfNotExists(IDatabase db, string content)
    {
        // if hasn't been added yet
        return db.SetAdd("items", content);
    }
}
