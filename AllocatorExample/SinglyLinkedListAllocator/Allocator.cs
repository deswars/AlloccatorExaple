using AllocatorInterface;
using MemoryModel;

namespace Allocators.SinglyLinkedListAllocator
{
    public class Allocator : IAllocator
    {
        public Allocator(Memory memory)
        {
            _memory = memory;
            // TODO first and last elements
        }

        public uint Alloc(uint size)
        {
            throw new System.NotImplementedException();
        }

        public bool Free(uint address)
        {
            throw new System.NotImplementedException();
        }

        protected const uint addressSize = sizeof(uint);
        protected const uint headerSize = addressSize * 2;

        protected Memory _memory;
    }
}
