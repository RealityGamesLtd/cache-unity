using System;

namespace Cache.Data
{
    public abstract class Cachable
    {
        public DateTime? Date { get; set; }
        public bool DoesNotAutoExpire => Date == null;

#if UNITY_EDITOR
        public abstract dynamic GetObject();
#endif
    }
}
