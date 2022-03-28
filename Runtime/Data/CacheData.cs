using System;

namespace Cache.Data
{
    public abstract class CacheData<T> :
        Cachable, IDisposable, ICountable
    {
        public bool IsFree { get { return ReferenceCount <= 0; } }
        public int ReferenceCount { get; private set; }
        public T Data { get; private set; }

        public CacheData(T data)
        {
            ReferenceCount = 0;
            Data = data;
        }

        public virtual void Dispose() {}

        public void Aquire()
        {
            ReferenceCount++;
        }

        public void Release()
        {
            if (--ReferenceCount < 0)
                ReferenceCount = 0;
        }

        public override UnityEngine.Object GetObject()
        {
            return Data as UnityEngine.Object;
        }
    }
}
