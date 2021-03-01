using System;
using System.Collections;
using System.Text.RegularExpressions;
using Cache;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class UrlCacheTests
    {
        private UrlCache cache = new UrlCache();

        [SetUp]
        public void SetupCache()
        {
            cache = new UrlCache();
        }

        [UnityTest]
        public IEnumerator CachedWithNoTTLNotExpiresImmediately()
        {
            var key = Guid.NewGuid().ToString();
            var data = "ASDF";
            cache.PutIntoCache(key, data);

            yield return new WaitForSeconds(1);

            var wasCacheHit = cache.GetFromCache(key, out string retrievedData);

            Assert.IsTrue(wasCacheHit);
            Assert.IsNotNull(retrievedData);
            Assert.AreEqual(data, retrievedData);
        }

        [UnityTest]
        public IEnumerator CachedWithTTLExpires()
        {
            var key = Guid.NewGuid().ToString();
            var data = "ASDF";
            cache.PutIntoCache(key, data, TimeSpan.FromMilliseconds(1));

            yield return new WaitForSeconds(1);

            var wasCacheHit = cache.GetFromCache(key, out string retrievedData);

            LogAssert.Expect(LogType.Error, new Regex("^Cache expired.+"));
            Assert.IsFalse(wasCacheHit);
            Assert.IsNull(retrievedData);
        }

        [Test]
        public void WillNotMatchWhenNoRulesWereAdded()
        {
            Assert.IsFalse(cache.MatchRules(null));
            Assert.IsFalse(cache.MatchRules(string.Empty));
            Assert.IsFalse(cache.MatchRules(""));
            Assert.IsFalse(cache.MatchRules("SomeString"));
        }

        [Test]
        public void WillMatchWhenShould()
        {
            cache.AddRule(new Regex(".png$"));
            var wasMatch = cache.MatchRules("Obrazek.png");

            Assert.IsTrue(wasMatch);
        }

        [Test]
        public void WillNotMatchWhenShouldNot()
        {
            cache.AddRule(new Regex(".jpg$"));
            var wasMatch = cache.MatchRules("Obrazek.png");

            Assert.IsFalse(wasMatch);
        }
    }
}
