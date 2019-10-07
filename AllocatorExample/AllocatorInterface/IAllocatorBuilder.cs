using MemoryModel;

namespace AllocatorInterface
{
    public interface IAllocatorBuilder
    {
        IAllocator Build(Memory memory);
    }
}
