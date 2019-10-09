using AllocatorInterface;
using MemoryModel;

namespace Allocators.SinglyLinkedListAllocator
{
    class NaiveRellocableAllocatorBuilder : IAllocatorRellocableBuilder
    {
        public void SetMemory(Memory memory)
        {
            _memory = memory;
        }

        public IAllocator Build()
        {
            return new NaiveRellocableAllocator(_memory);
        }

        public IAllocatorReallocable BuildRellocable()
        {
            return new NaiveRellocableAllocator(_memory);
        }

        public IAllocatorAnalizer BuildAnalizer()
        {
            return new AllocatorAnalizer(_memory);
        }

        private Memory _memory;
    }
}
