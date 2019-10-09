using AllocatorInterface;
using MemoryModel;

namespace Allocators.SinglyLinkedListAllocator
{
    public class NaiveRellocableAllocator : Allocator, IAllocatorReallocable
    {
        public NaiveRellocableAllocator(Memory memory) : base(memory) { }

        public uint Relloc(uint address, uint newSize)
        {
            uint newAddress = Alloc(newSize);
            if (newAddress != Null)
            {
                uint oldHeader = GetBlockHeaderAddress(address);
                uint oldMixed = GetBlockMixed(oldHeader);
                uint oldSize = GetSize(oldMixed);
                uint copySize = newSize > oldSize ? oldSize : newSize;
                _memory.Copy(newAddress, address, copySize);
                Free(address);
                return newAddress;
            }
            return Null;
        }
    }
}
