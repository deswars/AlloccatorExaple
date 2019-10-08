namespace AllocatorInterface
{
    public enum MemoryStatus
    {
        Free = 0, Busy = 1, System = 2, Reserved = 3
    }

    public interface IAllocator
    {
        uint Null { get; }

        uint Alloc(uint size);
        bool Free(uint address);
    }
}
