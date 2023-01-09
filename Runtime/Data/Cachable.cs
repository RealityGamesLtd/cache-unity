using System;

namespace Cache.Data
{
    public abstract class Cachable
    {
        public bool DoesNotAutoExpire => Date == null;
        public DateTime? Date { get; set; }
        public string ID { get; protected set; }
        public Action<string> OnReleased;

        public virtual void Release()
        {
            OnReleased?.Invoke(ID);
        }

#if UNITY_EDITOR
        public abstract dynamic GetObject();
#endif
    }
}
