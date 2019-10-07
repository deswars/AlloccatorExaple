using AllocatorInterface;
using MemoryModel;

namespace Allocators.SinglyLinkedListAllocator
{
    public class AllocatorBuilder : IAllocatorBuilder
    {
        public IAllocator Build(Memory memory)
        {
            return new Allocator(memory);
        }
    }
}
