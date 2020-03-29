using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace TrueLayer.TransactionData.Services.Data.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static async Task SetValueObject(this IDistributedCache cache, string key, object value, TimeSpan lifespan = default)
        {
            if (lifespan == default)
            {
                await cache.SetAsync(key,
                    value is string s ? EncodeObject(s) : EncodeObject(JsonConvert.SerializeObject(value)));
            }
            else
            {
                await cache.SetAsync(key,
                    value is string s ? EncodeObject(s) : EncodeObject(JsonConvert.SerializeObject(value)),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = lifespan
                    });
            }

            ;
        }

        public static async Task<TObjectType> Get<TObjectType>(this IDistributedCache cache, string key)
        where TObjectType: class, new()
        {
            var cachedItemBytes = await cache.GetAsync(key);

                if (cachedItemBytes == null)
                    return null;

                var value = DecodeObject(cachedItemBytes);

                if (typeof(TObjectType) == typeof(string))
                    return value as TObjectType;

                return JsonConvert.DeserializeObject<TObjectType>(value);
        }
        
        
        private static byte[] EncodeObject(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        private static string DecodeObject(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}