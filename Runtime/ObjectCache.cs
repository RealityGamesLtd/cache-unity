using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Cache
{
    public interface ICache
    {
        bool GetFromCache<T>(string key, out T data) where T : class;
        void PutIntoCache<T>(string key, T input) where T : class;
        void ExpireCacheData(string key);
        void ExpireAllCachedData();

        TimeSpan? TTL { get; }
        int Size { get; }
    }

    public class ObjectCache : ICache, IDisposable
    {
        public int Size { get; private set; }
        public TimeSpan? TTL { get; private set; }

        private readonly Dictionary<string, CacheData> cachedData = new Dictionary<string, CacheData>();
        private Timer timer;

        public ObjectCache(int size, TimeSpan? ttl = null)
        {
            Size = size;
            TTL = ttl;

            Dispose();
            timer = new Timer();
            timer.Elapsed += HandleExpiration;
            timer.AutoReset = false;
            timer.Stop();
        }

        public bool GetFromCache<T>(string url, out T data)
            where T : class
        {
            cachedData.TryGetValue(url, out var dataFromCache);
            data = null;

            if (dataFromCache == null)
                return false;

            if (!dataFromCache.DoesNotAutoExpire && dataFromCache.Date < DateTime.Now)
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

        public bool Contains(string url)
        {
            return cachedData.ContainsKey(url);
        }

        public void ExpireCacheData(string url)
        {
            if (cachedData.ContainsKey(url))
            {
                cachedData.Remove(url);
            }
        }

        public void PutIntoCache<T>(string url, T input)
            where T : class
        {
            if (input == null) return;

            HandleExpiration(null, null);

            CacheData data = new CacheData(input, TTL == null ? null : DateTime.Now + TTL);

            if (data == null) return;
            if (cachedData.ContainsKey(url))
            {
                cachedData.Remove(url);
            }

            if (cachedData.Count < Size)
            {
                cachedData.Add(url, data);
                UpdateInterval();
            }
        }

        private void UpdateInterval()
        {
            var timeStamp = GetClosestTimeStamp();
            if (timeStamp != null)
            {
                var diff = timeStamp.Value.Subtract(DateTime.Now);
                var time = diff.TotalMilliseconds;
                var offset = time < 1 ? 500f : time;
                timer.Interval = offset;
                timer.Start();
            }
            else
                timer.Stop();
        }

        private DateTime? GetClosestTimeStamp()
        {
            return cachedData.Min(x => x.Value.Date);
        }

        private void HandleExpiration(object sender, ElapsedEventArgs e)
        {
            List<string> toRemove = new List<string>();
            foreach (var pair in cachedData)
            {
                var url = pair.Key;
                var data = pair.Value;

                if (data.DoesNotAutoExpire) continue;
                if (!(data.Date <= DateTime.Now)) continue;

                toRemove.Add(url);
            }

            if(toRemove.Count > 0)
            {
                foreach (var url in toRemove)
                {
                    if (cachedData.ContainsKey(url))
                        cachedData.Remove(url);
                }

                UpdateInterval();
            }
        }

        public void ExpireAllCachedData()
        {
            foreach (var pair in cachedData)
            {
                ExpireCacheData(pair.Key);
            }
        }

        public void Dispose()
        {
            timer?.Close();
            timer?.Dispose();
        }
    }
}