using AllocatorInterface;
using MemoryModel;

namespace Allocators.SLLAllocator
{
    class NaiveReallocableAllocatorBuilder : IAllocatorReallocableBuilder
    {
        public void SetMemory(Memory memory)
        {
            _memory = memory;
        }

        public IAllocator Build()
        {
            return new NaiveReallocableAllocator(_memory);
        }

        public IAllocatorReallocable BuildReallocable()
        {
            return new NaiveReallocableAllocator(_memory);
        }

        public IAllocatorAnalizer BuildAnalizer()
        {
            return new AllocatorAnalizer(_memory);
        }

        private Memory _memory;
    }
}
