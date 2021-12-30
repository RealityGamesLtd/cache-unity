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

        [UnityTest]
        public IEnumerator ExpirationTTLCacheTest()
        {
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            var key3 = Guid.NewGuid().ToString();
            var data = "ASDF";
            var cache = new ObjectCache(20, TimeSpan.FromSeconds(10));

            cache.PutIntoCache(key1, data);
            yield return new WaitForSeconds(3);
            cache.PutIntoCache(key2, data);
            yield return new WaitForSeconds(4);
            cache.PutIntoCache(key3, data);

            var wasCacheHit = cache.Contains(key1);
            Assert.IsTrue(wasCacheHit);
            wasCacheHit = cache.Contains(key2);
            Assert.IsTrue(wasCacheHit);
            wasCacheHit = cache.Contains(key3);
            Assert.IsTrue(wasCacheHit);

            yield return new WaitForSeconds(4);

            wasCacheHit = cache.Contains(key1);
            Assert.IsFalse(wasCacheHit);
            wasCacheHit = cache.Contains(key2);
            Assert.IsTrue(wasCacheHit);
            cache.Contains(key3);
            Assert.IsTrue(wasCacheHit);

            yield return new WaitForSeconds(4);

            wasCacheHit = cache.Contains(key1);
            Assert.IsFalse(wasCacheHit);
            wasCacheHit = cache.Contains(key2);
            Assert.IsFalse(wasCacheHit);
            wasCacheHit = cache.Contains(key3);
            Assert.IsTrue(wasCacheHit);

            yield return new WaitForSeconds(4);

            wasCacheHit = cache.Contains(key1);
            Assert.IsFalse(wasCacheHit);
            wasCacheHit = cache.Contains(key2);
            Assert.IsFalse(wasCacheHit);
            wasCacheHit = cache.Contains(key3);
            Assert.IsFalse(wasCacheHit);
        }

        [UnityTest]
        public IEnumerator CachedWithNoTTLNotExpiresImmediately()
        {
            var key = Guid.NewGuid().ToString();
            var data = "ASDF";
            var cache = new ObjectCache(20, null);

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
            var cache = new ObjectCache(20, TimeSpan.FromMilliseconds(1));
            cache.PutIntoCache(key, data);

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
            var cache = new ObjectCache(20, TimeSpan.FromMilliseconds(1));
            cache.PutIntoCache(key, data);
            var wasCacheHit = cache.GetFromCache(key, out string retrievedData);

            Assert.IsFalse(wasCacheHit);
            Assert.IsNull(retrievedData);
        }
    }
}
