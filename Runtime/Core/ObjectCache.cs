using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Cache.Data;
using UniRx;

namespace Cache.Core
{
    public interface ICache
    {
        bool GetFromCache<T>(string key, out T data) where T : Cachable;
        void PutIntoCache(string key, Cachable input);
        void ExpireCacheData(string key);
        void ExpireAllCachedData();

        TimeSpan? TTL { get; }
        int Size { get; }
    }

    public class ObjectCache : ICache, IDisposable
    {
        public int Size { get; private set; }
        public TimeSpan? TTL { get; private set; }

        private readonly Dictionary<string, Cachable> cachedData = new Dictionary<string, Cachable>();
        private readonly Timer timer;

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
            where T : Cachable
        {
            cachedData.TryGetValue(url, out var dataFromCache);
            data = null;

            if (dataFromCache == null)
                return false;

            if (!dataFromCache.DoesNotAutoExpire && dataFromCache.Date < DateTime.Now)
            {
                ExpireCacheData(url);
                return false;
            }

            if (!(dataFromCache is T))
            {
                UnityEngine.Debug.LogError($"Cache not in provided type {typeof(T)}");
                return false;
            }

            data = dataFromCache as T;
            return true;
        }

        public bool Contains(string url)
        {
            return !string.IsNullOrEmpty(url) && cachedData.ContainsKey(url);
        }

        public void ExpireCacheData(string url)
        {
            if (url == null) return;

            ExpireElement(url);
        }

        public void PutIntoCache(string url, Cachable input)
        {
            if (input == null) return;
            if (cachedData.ContainsKey(url)) return;

            input.Date = TTL == null ? null : DateTime.Now + TTL;

            if (cachedData.Count < Size)
            {
                cachedData.Add(url, input);
                UpdateInterval();
            }
        }

        private void UpdateInterval()
        {
            DateTime timeStamp = default;
            if (cachedData != null && cachedData.Count > 0)
                timeStamp = cachedData.Min(x => x.Value.Date ?? default);

            if (!timeStamp.Equals(default) && timeStamp != null)
            {
                var diff = timeStamp.Subtract(DateTime.Now);
                timer.Interval = diff.TotalMilliseconds < 1 ? 500f : diff.TotalMilliseconds;
                timer.Start();
            }
            else
                timer.Stop();
        }

        private void HandleExpiration(object sender, ElapsedEventArgs e)
        {
            MainThreadDispatcher.Send((obj) => ExpireCoroutine(), null);
        }

        private void ExpireCoroutine()
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

            foreach (var url in toRemove)
                ExpireElement(url);

            UpdateInterval();
        }

        private void ExpireElement(string url)
        {
            if (cachedData.TryGetValue(url, out var cachable))
            {               
                var isAlive = (cachable as ICountable).IsAlive;
                if (!isAlive)
                    (cachable as IDisposable).Dispose();

                cachedData.Remove(url);
            }
        }

        public void ExpireAllCachedData()
        {
            foreach (var pair in cachedData)
                ExpireCacheData(pair.Key);
        }

        public void Dispose()
        {
            timer?.Close();
            timer?.Dispose();
        }
    }
}