using AllocatorInterface;
using MemoryModel;

namespace Allocators.SinglyLinkedListAllocator
{
    public class Allocator : IAllocator
    {
        //All addresses are multile of addressSize
        //Block format: NextBlockAddress|Mixed|Data
        //Mixed = Size & Status
        public Allocator(Memory memory)
        {
            _memory = memory;
            uint size = _memory.GetSize();

            uint firstNext = size - headerSize;
            uint firstSize = size - 2 * headerSize;
            uint firstMixed = GetMixed(firstSize, MemoryStatus.Free);
            SetBlockHeader(0, firstNext, firstMixed);

            uint lastNext = Null;
            uint lastSize = 0;
            uint lastMixed = GetMixed(lastSize, MemoryStatus.System);
            SetBlockHeader(firstNext, lastNext, lastMixed);
        }

        public uint Null
        {
            get
            {
                return 0;
            }
        }

        public uint Alloc(uint size)
        {
            //round up requested size
            uint requiredSize = (size + addressSize - 1) & sizeMask;

            uint currentAddres = 0;
            uint nextAddress = GetBlockNext(currentAddres);
            uint blockMixed = GetBlockMixed(currentAddres);
            uint blockSize = GetSize(blockMixed);
            MemoryStatus blockStatus = GetStatus(blockMixed);

            while (nextAddress != Null)
            {
                if (blockStatus == MemoryStatus.Free)
                {
                    if (blockSize >= requiredSize)
                    {
                        if (blockSize <= requiredSize + headerSize)
                        {
                            AllocAllBlock(currentAddres);
                        }
                        else
                        {
                            AllocPartialBlock(currentAddres, requiredSize);
                        }
                        return GetBlockData(currentAddres);
                    }
                }
                currentAddres = nextAddress;
                nextAddress = GetBlockNext(currentAddres);
                blockMixed = GetBlockMixed(currentAddres);
                blockSize = GetSize(blockMixed);
                blockStatus = GetStatus(blockMixed);
            }
            return Null;
        }

        public bool Free(uint address)
        {
            throw new System.NotImplementedException();
        }

        protected const uint addressSize = sizeof(uint);
        protected const uint headerSize = addressSize * 2;
        protected const uint statusMask = addressSize - 1;
        protected const uint sizeMask = ~statusMask;

        protected Memory _memory;

        protected uint GetSize(uint mixedValue)
        {
            return mixedValue & sizeMask;
        }

        protected MemoryStatus GetStatus(uint mixedValue)
        {
            return (MemoryStatus)(mixedValue & statusMask);
        }

        protected uint GetMixed(uint size, MemoryStatus status)
        {
            return size | (uint)status;
        }

        protected uint GetBlockNext(uint address)
        {
            return _memory.ReadWord(address);
        }

        protected uint GetBlockMixed(uint address)
        {
            return _memory.ReadWord(address + addressSize);
        }

        protected uint GetBlockData(uint address)
        {
            return address + headerSize;
        }

        protected void SetBlockMixed(uint address, uint mixed)
        {
            _memory.WriteWord(address + addressSize, mixed);
        }

        protected void SetBlockNext(uint address, uint next)
        {
            _memory.WriteWord(address, next);
        }

        protected void SetBlockHeader(uint address, uint next, uint mixed)
        {
            SetBlockNext(address, next);
            SetBlockMixed(address, mixed);
        }

        protected void AllocAllBlock(uint address)
        {
            uint mixed = GetBlockMixed(address);
            uint size = GetSize(mixed);
            mixed = GetMixed(size, MemoryStatus.Busy);
            SetBlockMixed(address, mixed);
        }

        protected void AllocPartialBlock(uint address, uint size)
        {
            uint nextBlock = GetBlockNext(address);
            uint currentMixed = GetBlockMixed(address);
            uint currentSize = GetSize(currentMixed);

            uint freeAddress = address + headerSize + size;
            uint freeSize = currentSize - size - headerSize;
            uint freeMixed = GetMixed(freeSize, MemoryStatus.Free);
            SetBlockHeader(freeAddress, nextBlock, freeMixed);

            uint busyMixed = GetMixed(size, MemoryStatus.Busy);
            SetBlockHeader(address, freeAddress, busyMixed);
        }
    }
}
