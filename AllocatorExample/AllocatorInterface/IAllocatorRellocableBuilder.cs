using MemoryModel;

namespace AllocatorInterface
{
    public interface IAllocatorRellocableBuilder
    {
        IAllocator Build(Memory memory);
    }
}
