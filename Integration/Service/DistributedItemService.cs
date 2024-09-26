using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service;

public class DistributedItemService
{
    private readonly IDatabase _redisDb;

    public DistributedItemService(IDatabase redisDb)
    {
        _redisDb = redisDb;
    }

    public async Task<string> SaveItemAsync(string content)
    {
        var lockKey = $"lock:{content}";
        var lockToken = Guid.NewGuid().ToString();

        // Acquire a distributed lock
        var lockAcquired = await _redisDb.StringSetAsync(lockKey, lockToken, TimeSpan.FromSeconds(40), When.NotExists);

        if (!lockAcquired)
        {
            throw new InvalidOperationException("Item already exists.");
        }

        try
        {
            // Simulate a delay for processing
            await Task.Delay(2000);

            // Save the item in Redis
            await _redisDb.StringSetAsync($"item:{content}", content);
            return $"Item with content '{content}' saved successfully.";
        }
        finally
        {
            // Release the lock
            var tokenValue = await _redisDb.StringGetAsync(lockKey);
            if (tokenValue == lockToken)
            {
                await _redisDb.KeyDeleteAsync(lockKey);
            }
        }
    }
}

