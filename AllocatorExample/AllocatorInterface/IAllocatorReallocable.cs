namespace AllocatorInterface
{
    public interface IAllocatorReallocable : IAllocator
    {
        public uint Relloc(uint address, uint newSize);
    }
}
