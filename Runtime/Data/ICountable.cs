namespace Cache.Data
{
    public interface ICountable
    {
        public bool IsAlive { get; }
        void Release();
        void Aquire();
    }
}
