namespace AllocatorInterface
{
    public interface IAllocatorReallocable : IAllocator
    {
        public uint Realloc(uint address, uint newSize);
    }
}
