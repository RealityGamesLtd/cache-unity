using System;
using System.Collections;
using System.Text.RegularExpressions;
using Cache.Core;
using Cache.Data;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class UrlCacheTests
    {
        public class CachedString : CacheData<string>
        {
            public CachedString(string data, string url) : base(data, url) {}
        }

        [SetUp]
        public void SetUp()
        {
            MainThreadDispatcher.Initialize();
        }

        [UnityTest]
        public IEnumerator CachedWithNoTTLNotExpiresImmediately()
        {
            var key = Guid.NewGuid().ToString();
            var data = new CachedString("ASDF", key);
            var cache = new UrlCache(20, null);
            cache.PutIntoCache(key, data);

            yield return new WaitForSeconds(1);

            var wasCacheHit = cache.GetFromCache(key, out CachedString retrievedData);

            Assert.IsTrue(wasCacheHit);
            Assert.IsNotNull(retrievedData);
            Assert.AreEqual(data, retrievedData);
        }

        [UnityTest]
        public IEnumerator CachedWithTTLExpires()
        {
            var key = Guid.NewGuid().ToString();
            var data = new CachedString("ASDF", key);
            var cache = new UrlCache(20, TimeSpan.FromMilliseconds(1));
            cache.PutIntoCache(key, data);

            yield return new WaitForSeconds(1);

            var wasCacheHit = cache.GetFromCache(key, out CachedString retrievedData);

            Assert.IsFalse(wasCacheHit);
            Assert.IsNull(retrievedData);
        }

        [Test]
        public void WillNotMatchWhenNoRulesWereAdded()
        {
            var cache = new UrlCache(20, null);
            Assert.IsFalse(cache.MatchRules(null));
            Assert.IsFalse(cache.MatchRules(string.Empty));
            Assert.IsFalse(cache.MatchRules(""));
            Assert.IsFalse(cache.MatchRules("SomeString"));
        }

        [Test]
        public void WillMatchWhenShould()
        {
            var cache = new UrlCache(20, null);
            cache.AddRule(new Regex(".png$"));
            var wasMatch = cache.MatchRules("Obrazek.png");

            Assert.IsTrue(wasMatch);
        }

        [Test]
        public void WillNotMatchWhenShouldNot()
        {
            var cache = new UrlCache(20, null);
            cache.AddRule(new Regex(".jpg$"));
            var wasMatch = cache.MatchRules("Obrazek.png");

            Assert.IsFalse(wasMatch);
        }
    }
}
