namespace OctoberStudio.Pool
{
    public abstract class AbstractPool
    {
        private int hash;

        public AbstractPool(string name)
        {
            hash = name.GetHashCode();
        }

        public bool CompareHash(int hash)
        {
            return hash == this.hash;
        }
    }
}