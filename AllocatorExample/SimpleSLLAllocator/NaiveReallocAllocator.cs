using AllocatorInterface;
using MemoryModel;

namespace Allocators.SimpleSLLAllocator
{
    // Just alloc new block, copy data and free old block
    class NaiveReallocAllocator : Allocator, IAllocatorReallocable
    {
        public NaiveReallocAllocator(Memory memory) : base(memory)
        {
        }

        public uint Realloc(uint address, uint newSize)
        {
            uint currSize = RoundUpSize(newSize);
            Header header = Header.ReadDataHeader(_memory, address);

            if (currSize == header.Size)
            {
                return address;
            }
            uint newBlock = Alloc(newSize);
            if (newBlock == Null)
            {
                return Null;
            }

            _memory.Copy(newBlock, address, header.Size > newSize ? newSize : header.Size);
            Free(address);
            return newBlock;
        }
    }
}
