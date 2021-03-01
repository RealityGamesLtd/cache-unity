using System;

namespace Cache
{
    public class CacheData
    {
        public CacheData(object data, DateTime? date)
        {
            Data = data;
            Date = date;
        }

        public object Data { get; private set; }
        public DateTime? Date { get; private set; }
        public bool DoesNotAutoExpire => Date == null;
    }
}
