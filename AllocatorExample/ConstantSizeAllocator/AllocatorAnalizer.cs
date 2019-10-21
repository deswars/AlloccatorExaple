using AllocatorInterface;
using MemoryModel;

namespace Allocators.ConstantSizeAllocator
{
    public class AllocatorAnalizer : IAllocatorAnalizer
    {
        public AllocatorAnalizer(Memory memory, uint blockSize)
        {
            _memory = memory;
            _blockSize = blockSize;
        }

        public uint Null
        {
            get
            {
                return 0;
            }
        }

        public MemoryAnalizerStatus[] AnalizeMemory()
        {
            if (_memory == null)
            {
                return null;
            }
            var result = new MemoryAnalizerStatus[_memory.GetSize()];

            uint blockCount = _memory.GetSize() / _blockSize;
            uint headerSize = (blockCount + 7) / 8;
            uint reservedBlocks = (headerSize + _blockSize - 1) / _blockSize;

            for (uint i = 0; i < _memory.GetSize(); i+= _blockSize)
            {
                if (i < reservedBlocks * _blockSize)
                {
                    for (uint j = 0; j < _blockSize; j++)
                    {
                        result[i + j] = MemoryAnalizerStatus.Header;
                    }
                }
                else
                {
                    MemoryAnalizerStatus status;
                    if (IsAllocated(i))
                    {
                        status = MemoryAnalizerStatus.Data;
                    }
                    else
                    {
                        status = MemoryAnalizerStatus.Free;
                    }
                    for (uint j = 0; j < _blockSize; j++)
                    {
                        result[i + j] = status;
                    }
                }
            }
            return result;
        }

        private static readonly byte[] MaskBit = { 0b00000001, 0b00000010, 0b00000100, 0b00001000, 0b00010000, 0b00100000, 0b01000000, 0b10000000 };
        private const byte BusyByte = 255;

        private readonly Memory _memory;
        private uint _blockSize;

        private bool IsAllocated(uint address)
        {
            uint block = address / _blockSize;
            uint headerByte = block / 8;
            uint headerBit = block % 8;
            byte statusByte = _memory.Read(headerByte);
            if ((statusByte & MaskBit[headerBit]) != 0)
            {
                return true;
            }
            return false;
        }
    }
}
