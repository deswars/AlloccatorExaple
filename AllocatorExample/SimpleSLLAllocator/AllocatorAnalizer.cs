using AllocatorInterface;
using MemoryModel;
using System.Collections.Generic;

namespace Allocators.SimpleSLLAllocator
{
    public class AllocatorAnalizer : IAllocatorAnalizer
    {
        public AllocatorAnalizer(Memory memory)
        {
            _memory = memory;
        }

        public static uint Null
        {
            get
            {
                return uint.MaxValue;
            }
        }

        public MemoryAnalizerStatus[] AnalizeMemory()
        {
            if (_memory == null)
            {
                return null;
            }
            var result = new MemoryAnalizerStatus[_memory.GetSize()];

            Header currHeader = Header.Read(_memory, 0);
            while (currHeader.Status != MemoryStatus.System)
            {
                CheckBlock(currHeader, result);
                currHeader = Header.Read(_memory, currHeader.NextAddress);
            }
            CheckFinalHeader(currHeader, result);
            return result;
        }

        public BlockStatus HighLevelAnalizeMemory()
        {
            if (_memory == null)
            {
                return null;
            }
            uint index = 0;
            uint size = _memory.GetSize();
            MemoryAnalizerStatus status = MemoryAnalizerStatus.SpecialHeader;
            List<BlockStatus> children = new List<BlockStatus>();

            uint childIndex = 0;
            Header currHeader = Header.Read(_memory, 0);
            BlockStatus blockStatus = GetBlockStatus(currHeader, childIndex);
            children.Add(blockStatus);
            while (currHeader.Status != MemoryStatus.System)
            {
                childIndex++;
                currHeader = Header.Read(_memory, currHeader.NextAddress);
                blockStatus = GetBlockStatus(currHeader, childIndex);
                children.Add(blockStatus);
            }

            BlockStatus memoryBlock = new BlockStatus(index, size, status, children);
            return memoryBlock;
        }

        private readonly Memory _memory;

        private void CheckBlock(Header header, MemoryAnalizerStatus[] result)
        {
            for (uint i = header.Address; i < header.DataAddress; i++)
            {
                result[i] = MemoryAnalizerStatus.Header;
            }
            MemoryAnalizerStatus dataStatus;
            if (header.Status == MemoryStatus.Free)
            {
                dataStatus = MemoryAnalizerStatus.Free;
            }
            else
            {
                dataStatus = MemoryAnalizerStatus.Data;
            }
            for (uint i = header.DataAddress; i < header.DataAddress + header.Size; i++)
            {
                result[i] = dataStatus;
            }
        }

        private void CheckFinalHeader(Header header, MemoryAnalizerStatus[] result)
        {
            for (int i = 0; i < Header.HeaderSize; i++)
            {
                result[header.Address + i] = MemoryAnalizerStatus.SpecialHeader;
            }
        }

        private BlockStatus GetBlockStatus(Header header, uint index)
        {
            MemoryAnalizerStatus status;
            if (header.Status == MemoryStatus.System)
            {
                status = MemoryAnalizerStatus.SpecialHeader;
            }
            else if (header.Status == MemoryStatus.Free)
            {
                status = MemoryAnalizerStatus.Free;
            }
            else
            {
                status = MemoryAnalizerStatus.Data;
            }
            return new BlockStatus(index, header.Size + Header.HeaderSize, status, null);
        }
    }
}
