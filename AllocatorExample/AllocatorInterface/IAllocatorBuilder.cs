using MemoryModel;

namespace AllocatorInterface
{
    interface IAllocatorBuilder
    {
        IAllocator Build(Memory memory);
    }
}
