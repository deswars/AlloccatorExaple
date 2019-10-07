namespace AllocatorInterface
{
    interface IAllocator
    {
        uint Alloc(uint size);
        bool Free(uint address);
    }
}
