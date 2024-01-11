using System.Threading;
using System.Threading.Tasks;
using Core.Enums;
using Core.Helpers;
using Core.Interfaces;
using Core.Models;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Core.Behaviours
{
    public class CacheBehaviour<TRequest, TResponse>(
        IOptions<CoreSettings> options,
        IMemoryCache cache,
        IDistributedCache distributedCache,
        ISerializerHelper serializerHelper)
        : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly CoreSettings _coreSettings = options.Value;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICacheable cacheable || !_coreSettings.Cache.IsActive) return await next();

            var key = cacheable.CacheSettings.Key.CreateCacheKey(request.GetType().Name);

            return cacheable.CacheType switch
            {
                CacheType.None => await next(),
                CacheType.Memory => await GetFromMemoryCache(key, cacheable, next),
                CacheType.Distributed => await GetFromDistributedCache(key, cacheable, next),
                _ => await next()
            };
        }

        private async Task<TResponse> GetFromMemoryCache(string key, ICacheable cacheable, RequestHandlerDelegate<TResponse> next)
        {
            var isExist = cache.TryGetValue(key, out TResponse response);

            if (isExist)
            {
                return response;
            }

            response = await next();

            if (response == null) return default;

            cache.Set(key, response, cacheable.CacheSettings.Value);

            return response;
        }

        private async Task<TResponse> GetFromDistributedCache(string key, ICacheable cacheable, RequestHandlerDelegate<TResponse> next)
        {
            var value = await distributedCache.GetAsync(key);

            if (value != null)
            {
                return serializerHelper.Deserialize<TResponse>(value);
            }

            var response = await next();

            if (response == null) return default;

            var option = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheable.CacheSettings.Value
            };

            await distributedCache.SetAsync(key, serializerHelper.Serialize(response), option);

            return response;
        }
        
    }
}