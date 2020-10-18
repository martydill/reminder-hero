using System;
using System.Runtime.Caching;

namespace ReminderHero.Models
{
    public class MemoryCacheBase
    {
        private readonly MemoryCache _cache = MemoryCache.Default;

        protected MemoryCache Cache
        {
            get { return _cache; }
        }

        public T CachedRead<T>(string key, Func<object> retriever) where T : class
        {
            var entity = _cache.Get(key) as T;
            if (entity == null)
            {
                entity = retriever() as T;
                if (entity != null)
                    _cache.Set(key, entity, new DateTimeOffset(DateTime.Now.AddMinutes(10)));
            }

            return entity;
        }

        public void Remove(string key)
        {
            if(_cache.Contains(key))
                _cache.Remove(key);
        }
    }
}

