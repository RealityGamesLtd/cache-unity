namespace Cache.Data
{
    public abstract class CacheData<T> :
        Cachable, ICountable
    {
        public bool IsFree => ReferenceCount <= 0;
        public int ReferenceCount { get; private set; }
        public T Data { get; private set; }

        public CacheData(T data, string url)
        {
            ReferenceCount = 0;
            Data = data;
            ID = url;
        }

        public void Aquire()
        {
            ReferenceCount++;
        }

        public override void Release()
        {
            if (--ReferenceCount <= 0)
            {
                ReferenceCount = 0;
                base.Release();
            }
        }

#if UNITY_EDITOR
        public override dynamic GetObject()
        {
            return Data;
        }
#endif
    }
}
