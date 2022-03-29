using System;
using System.Collections;
using Cache.Core;
using Cache.Data;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ObjectCacheTests
    {
        public class CachedString : CacheData<string>
        {
            public CachedString(string data) : base(data) { }
        }

        public class CachedTexture : CacheData<Texture2D>
        {
            public CachedTexture(Texture2D tex) : base(tex) { }

            public override void Dispose()
            {
                base.Dispose();
                UnityEngine.Object.Destroy(Data);
            }
        }

        [SetUp]
        public void SetUp()
        {
            MainThreadDispatcher.Initialize();
        }

        [UnityTest]
        public IEnumerator ExpirationTTLCacheTest()
        {
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            var key3 = Guid.NewGuid().ToString();
            var data = new CachedString("ASDF");
            var data2 = new CachedTexture(new Texture2D(120, 120));
            var data3 = new CachedString("ASDF");

            var cache = new ObjectCache(20, TimeSpan.FromSeconds(10));

            cache.PutIntoCache(key1, data);
            yield return new WaitForSeconds(3);
            cache.PutIntoCache(key2, data2);
            yield return new WaitForSeconds(4);
            cache.PutIntoCache(key3, data3);

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
            wasCacheHit = cache.Contains(key3);
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
            var data = new CachedString("ASDF");
            var cache = new ObjectCache(20, null);

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
            var data = new CachedString("ASDF");
            var cache = new ObjectCache(20, TimeSpan.FromMilliseconds(1));
            cache.PutIntoCache(key, data);

            yield return new WaitForSeconds(1);

            var wasCacheHit = cache.GetFromCache(key, out CachedString retrievedData);

            Assert.IsFalse(wasCacheHit);
            Assert.IsNull(retrievedData);
        }

        [Test]
        public void CachingNullWillNotHitCacheWhenGettingByKey()
        {
            var key = Guid.NewGuid().ToString();
            CachedString data = null;
            var cache = new ObjectCache(20, TimeSpan.FromMilliseconds(1));
            cache.PutIntoCache(key, data);
            var wasCacheHit = cache.GetFromCache(key, out CachedString retrievedData);

            Assert.IsFalse(wasCacheHit);
            Assert.IsNull(retrievedData);
        }
    }
}
