﻿using System;
using UnityEngine;

namespace Cache.Data
{
    public abstract class CacheData<T> :
        Cachable, IDisposable, ICountable
    {
        private int referenceCount;

        public CacheData(T data)
        {
            referenceCount = 0;
            Data = data;
        }

        public T Data { get; private set; }
        public bool IsAlive => referenceCount > 0;

        public virtual void Dispose() { }
        public void Aquire() => referenceCount++;
        public void Release()
        {
            if (--referenceCount < 0)
                referenceCount = 0;

            if (referenceCount <= 0)
                Dispose();
        }
    }
}
