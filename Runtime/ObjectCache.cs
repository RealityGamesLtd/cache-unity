using System;
using System.Collections.Generic;

namespace Cache
{
    public interface ICache
    {
        bool GetFromCache<T>(string key, out T data) where T : class;
        void PutIntoCache<T>(string key, T input) where T : class;
        void PutIntoCache<T>(string key, T input, TimeSpan ttl) where T : class;
        void ExpireCacheData(string key);
        void ExpireAllCachedData();
    }

    public class ObjectCache : ICache
    {
        private Dictionary<string, CacheData> cachedData = new Dictionary<string, CacheData>();

        public bool GetFromCache<T>(string url, out T data) where T : class
        {
            cachedData.TryGetValue(url, out var dataFromCache);
            data = null;

            if (dataFromCache == null)
            {
                return false;
            }

            if (dataFromCache.DoesNotAutoExpire == false && dataFromCache.Date.Value.Subtract(DateTime.Now).TotalMilliseconds < 0)
            {
                UnityEngine.Debug.LogError($"Cache expired {url}");
                ExpireCacheData(url);
                return false;
            }

            if (!(dataFromCache.Data is T))
            {
                UnityEngine.Debug.LogError($"Cache not in provided type {typeof(T)}");
                return false;
            }

            data = dataFromCache.Data as T;
            return true;
        }

        public void ExpireCacheData(string url)
        {
            if (cachedData.ContainsKey(url))
            {
                cachedData.Remove(url);
            }
        }

        public void PutIntoCache<T>(string url, T input) where T : class
        {
            if (input == null) return;

            CacheData data = new CacheData(input, null);

            if (data == null) return;

            if (cachedData.ContainsKey(url)) cachedData.Remove(url);

            cachedData.Add(url, data);
        }

        public void PutIntoCache<T>(string url, T input, TimeSpan ttl) where T : class
        {
            CacheData data = new CacheData(input, DateTime.Now + ttl);

            if (data == null) return;

            if (cachedData.ContainsKey(url)) cachedData.Remove(url);

            cachedData.Add(url, data);
        }

        public void ExpireAllCachedData()
        {
            foreach (var pair in cachedData)
            {
                ExpireCacheData(pair.Key);
            }
        }
    }
}