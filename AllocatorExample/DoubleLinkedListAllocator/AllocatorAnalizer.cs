using AllocatorInterface;
using MemoryModel;

namespace Allocators.DoubleLinkedListAllocator
{
    class AllocatorAnalizer : IAllocatorAnalizer
    {
        public AllocatorAnalizer(Memory memory)
        {
            _memory = memory;
        }

        public uint Null
        {
            get
            {
                return uint.MaxValue;
            }
        }

        public MemoryAnalizerStatus[] AnalizeMemory()
        {
            var result = new MemoryAnalizerStatus[_memory.GetSize()];

            uint current = 0;
            uint next = GetBlockNext(current);
            while (next != Null)
            {
                current = CheckBlock(current, result);
                next = GetBlockNext(current);
            }
            CheckFinalHeader(current, result);
            return result;
        }

        private const uint addressSize = sizeof(uint);
        private const uint headerSize = 3 * addressSize;
        private const uint statusMask = addressSize - 1;
        private const uint sizeMask = ~statusMask;

        private readonly Memory _memory;

        private uint GetBlockNext(uint address)
        {
            return _memory.ReadWord(address);
        }

        private uint GetBlockMixed(uint address)
        {
            return _memory.ReadWord(address + 2 * addressSize);
        }

        private MemoryStatus GetBlockStatus(uint address)
        {
            return (MemoryStatus)(GetBlockMixed(address) & statusMask);
        }

        private uint GetBlockSize(uint address)
        {
            return GetBlockMixed(address) & sizeMask;
        }

        private uint CheckBlock(uint address, MemoryAnalizerStatus[] result)
        {
            uint dataAddress = address + headerSize;
            for (uint i = address; i < dataAddress; i++)
            {
                result[i] = MemoryAnalizerStatus.Header;
            }
            uint size = GetBlockSize(address);
            MemoryStatus blockStatus = GetBlockStatus(address);
            MemoryAnalizerStatus dataStatus;
            if (blockStatus == MemoryStatus.Free)
            {
                dataStatus = MemoryAnalizerStatus.Free;
            }
            else
            {
                dataStatus = MemoryAnalizerStatus.Data;
            }
            for (uint i = dataAddress; i < dataAddress + size; i++)
            {
                result[i] = dataStatus;
            }
            return GetBlockNext(address);
        }

        private void CheckFinalHeader(uint address, MemoryAnalizerStatus[] result)
        {
            for (int i = 0; i < headerSize; i++)
            {
                result[address + i] = MemoryAnalizerStatus.SpecialHeader;
            }
        }
    }
}
