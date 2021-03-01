using System;
using System.Collections;
using System.Text.RegularExpressions;
using Cache;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ObjectCacheTests
    {
        private ObjectCache cache = new ObjectCache();

        [SetUp]
        public void SetupCache()
        {
            cache = new ObjectCache();
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
        public void CachingNullWillNotHitCacheWhenGettingByKey()
        {
            var key = Guid.NewGuid().ToString();
            string data = null;
            cache.PutIntoCache(key, data);

            var wasCacheHit = cache.GetFromCache(key, out string retrievedData);

            Assert.IsFalse(wasCacheHit);
            Assert.IsNull(retrievedData);
        }
    }
}
