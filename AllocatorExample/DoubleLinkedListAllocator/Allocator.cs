using AllocatorInterface;
using MemoryModel;

namespace Allocators.DoubleLinkedListAllocator
{
    public class Allocator : IAllocator
    {
        //All addresses are multile of addressSize
        //Block format: NextBlockAddress|PrevBlockAddress|Mixed|Data
        //Mixed = Size & Status

        public Allocator(Memory memory)
        {
            _memory = memory;
            uint size = _memory.GetSize();

            uint firstNext = size - headerSize;
            uint firstPrev = Null;
            uint firstSize = size - 2 * headerSize;
            uint firstMixed = GetMixed(firstSize, MemoryStatus.Free);
            SetBlockHeader(0, firstNext, firstPrev, firstMixed);

            uint lastNext = Null;
            uint lastPrev = 0;
            uint lastSize = 0;
            uint lastMixed = GetMixed(lastSize, MemoryStatus.System);
            SetBlockHeader(firstNext, lastNext, lastPrev, lastMixed);
        }

        protected const uint addressSize = sizeof(uint);
        protected const uint headerSize = addressSize * 3;
        protected const uint statusMask = addressSize - 1;
        protected const uint sizeMask = ~statusMask;

        protected Memory _memory;

        public uint Null
        {
            get
            {
                return uint.MaxValue;
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
                        return GetBlockDataAddress(currentAddres);
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
        
        public void Free(uint address)
        {
            uint currentBlock = GetBlockHeaderAddress(address);
            uint currentMixed = GetBlockMixed(currentBlock);
            uint currentSize = GetSize(currentMixed);
            currentMixed = GetMixed(currentSize, MemoryStatus.Free);
            SetBlockMixed(currentBlock, currentMixed);
            UniteWithNext(currentBlock);
            UniteWithPrevious(currentBlock);
        }


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

        protected uint GetBlockPrev(uint address)
        {
            return _memory.ReadWord(address + addressSize);
        }

        protected uint GetBlockMixed(uint address)
        {
            return _memory.ReadWord(address + 2 * addressSize);
        }

        protected uint GetBlockDataAddress(uint address)
        {
            return address + headerSize;
        }

        protected uint GetBlockHeaderAddress(uint dataAddress)
        {
            return dataAddress - headerSize;
        }

        protected void SetBlockMixed(uint address, uint mixed)
        {
            _memory.WriteWord(address + 2 * addressSize, mixed);
        }

        protected void SetBlockNext(uint address, uint next)
        {
            _memory.WriteWord(address, next);
        }

        protected void SetBlockPrev(uint address, uint next)
        {
            _memory.WriteWord(address + addressSize, next);
        }

        protected void SetBlockHeader(uint address, uint next, uint prev, uint mixed)
        {
            SetBlockNext(address, next);
            SetBlockPrev(address, prev);
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
            uint prevBlock = GetBlockPrev(address);
            uint currentMixed = GetBlockMixed(address);
            uint currentSize = GetSize(currentMixed);

            uint freeAddress = address + headerSize + size;
            uint freeSize = currentSize - size - headerSize;
            uint freeMixed = GetMixed(freeSize, MemoryStatus.Free);
            SetBlockHeader(freeAddress, nextBlock, address, freeMixed);

            uint busyMixed = GetMixed(size, MemoryStatus.Busy);
            SetBlockHeader(address, freeAddress, prevBlock, busyMixed);
        }

        protected void UniteWithNext(uint address)
        {
            uint nextAddress = GetBlockNext(address);
            uint nextMixed = GetBlockMixed(nextAddress);
            MemoryStatus nextStatus = GetStatus(nextMixed);
            if (nextStatus == MemoryStatus.Free)
            {
                uint nextnextAddress = GetBlockNext(nextAddress);
                SetBlockPrev(nextnextAddress, address);

                uint mixed = GetBlockMixed(address);
                uint size = GetSize(mixed);
                uint nextSize = GetSize(nextMixed);
                size += nextSize + headerSize;
                mixed = GetMixed(size, MemoryStatus.Free);
                uint prevBlock = GetBlockPrev(address);
                SetBlockHeader(address, GetBlockNext(nextAddress), prevBlock, mixed);
            }
        }

        protected void UniteWithPrevious(uint address)
        {
            uint prevAddress = GetBlockPrev(address);
            if (prevAddress != Null)
            {
                uint prevMixed = GetBlockMixed(prevAddress);
                MemoryStatus prevStatus = GetStatus(prevMixed);
                if (prevStatus == MemoryStatus.Free)
                {
                    uint nextAddress = GetBlockNext(address);
                    SetBlockPrev(nextAddress, prevAddress);

                    uint mixed = GetBlockMixed(address);
                    uint size = GetSize(mixed);
                    uint prevSize = GetSize(prevMixed);
                    size += prevSize + headerSize;
                    mixed = GetMixed(size, MemoryStatus.Free);
                    uint nextBlock = GetBlockNext(address);
                    SetBlockHeader(prevAddress, nextBlock, GetBlockPrev(prevAddress), mixed);
                }
            }
        }

    }
}  