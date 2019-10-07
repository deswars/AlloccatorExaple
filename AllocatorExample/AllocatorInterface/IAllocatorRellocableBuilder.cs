using MemoryModel;

namespace AllocatorInterface
{
    interface IAllocatorRellocableBuilder
    {
        IAllocator Build(Memory memory);
    }
}
