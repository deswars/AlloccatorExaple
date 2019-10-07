namespace AllocatorInterface
{
    interface IAllocatorReallocable : IAllocator
    {
        public uint Relloc(uint address, uint newSize);
    }
}
