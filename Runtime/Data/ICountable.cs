namespace Cache.Data
{
    public interface ICountable
    {
        public bool IsFree { get; }
        void Release();
        void Aquire();
    }
}
