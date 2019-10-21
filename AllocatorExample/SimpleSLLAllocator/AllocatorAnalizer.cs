using AllocatorInterface;
using MemoryModel;

namespace Allocators.SimpleSLLAllocator
{
    public class AllocatorAnalizer : IAllocatorAnalizer
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

            Header currHeader = Header.Read(_memory, 0);
            while (currHeader.Status != MemoryStatus.System)
            {
                CheckBlock(currHeader, result);
                currHeader = Header.Read(_memory, currHeader.NextAddress);
            }
            CheckFinalHeader(currHeader, result);
            return result;
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
    }
}
