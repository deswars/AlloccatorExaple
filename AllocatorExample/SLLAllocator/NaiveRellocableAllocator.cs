using AllocatorInterface;
using MemoryModel;

namespace Allocators.SLLAllocator
{
    public class NaiveRellocableAllocator : Allocator, IAllocatorReallocable
    {
        public NaiveRellocableAllocator(Memory memory) : base(memory) { }

        public uint Relloc(uint address, uint newSize)
        {
            uint newAddress = Alloc(newSize);
            if (newAddress != Null)
            {
                uint oldHeaderAddress = GetBlockHeaderAddress(address);
                Header oldHeader = ReadHeader(oldHeaderAddress);
                uint oldSize = oldHeader.GetSize();
                uint copySize = newSize > oldSize ? oldSize : newSize;
                _memory.Copy(newAddress, address, copySize);
                Free(address);
                return newAddress;
            }
            return Null;
        }
    }
}
