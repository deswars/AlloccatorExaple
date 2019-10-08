using MemoryModel;

namespace AllocatorInterface
{
    public interface IAllocatorBuilder
    {
        void SetMemory(Memory memory);
        IAllocator Build();
        IAllocatorAnalizer BuildAnalizer();
    }
}
