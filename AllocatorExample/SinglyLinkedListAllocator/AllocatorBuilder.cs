using AllocatorInterface;
using MemoryModel;

namespace Allocators.SinglyLinkedListAllocator
{
    public class AllocatorBuilder : IAllocatorBuilder
    {
        public void SetMemory(Memory memory)
        {
            _memory = memory;
        }

        public IAllocator Build()
        {
            return new Allocator(_memory);
        }

        public IAllocatorAnalizer BuildAnalizer()
        {
            return new AllocatorAnalizer(_memory);
        }

        private Memory _memory;
    }
}
