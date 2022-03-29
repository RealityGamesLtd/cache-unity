namespace Cache.Data
{
    public interface ICountable
    {
        public int ReferenceCount { get; }
        public bool IsFree { get; }
        void Release();
        void Aquire();
    }
}
