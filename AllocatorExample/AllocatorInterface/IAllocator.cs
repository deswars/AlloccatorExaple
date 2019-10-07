namespace AllocatorInterface
{
    public interface IAllocator
    {
        uint Alloc(uint size);
        bool Free(uint address);
    }
}
