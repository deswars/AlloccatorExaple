using AllocatorInterface;
using MemoryModel;
using System.Diagnostics.Contracts;

namespace Allocators.ConstantSizeAllocator
{
    public class Allocator : IAllocator
    {
        public Allocator(Memory memory, uint blockSize)
        {
            Contract.Requires(memory != null);

            _memory = memory;
            _blockSize = blockSize;
            _blockCount = memory.GetSize() / blockSize;

            _headerSize = (_blockCount + 7) / 8;
            _reservedBlocks = (_headerSize + blockSize - 1) / blockSize;

            for (uint i = 0; i < _reservedBlocks; i++)
            {
                AllocBlock(i);
            }

            uint blockCount = _blockCount;
            byte lastStatusByte = 0;
            while (blockCount % 8 != 0)
            {
                lastStatusByte /= 2;
                lastStatusByte &= 128;
                blockCount--;
            }
            _memory.Write(_headerSize - 1, lastStatusByte);
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
            if (size != _blockSize)
            {
                return Null;
            }
            for (uint i = 0; i < _headerSize; i++)
            {
                byte status = _memory.Read(i);
                if (status != BusyByte)
                {
                    uint freeBlock = i * 8;
                    while (status % 2 != 0)
                    {
                        freeBlock++;
                        status /= 2;
                    }
                    AllocBlock(freeBlock);
                    return (freeBlock * _blockSize);
                }
            }
            return Null;
        }

        public void Free(uint address)
        {
            uint blockNumber = address / _blockSize;
            FreeBlock(blockNumber);
        }

        private static readonly byte[] MaskAlloc = { 0b00000001, 0b00000010, 0b00000100 , 0b00001000 , 0b00010000, 0b00100000, 0b01000000, 0b10000000 };
        private static readonly byte[] MaskFree = { 0b11111110, 0b11111101, 0b11111011, 0b11110111, 0b11101111, 0b11011111, 0b10111111, 0b01111111 };
        private const byte BusyByte = 255;

        private readonly Memory _memory;
        private uint _blockSize;
        private uint _blockCount;
        private uint _headerSize;
        private uint _reservedBlocks;
        
        private void AllocBlock(uint position)
        {
            uint byteNumber = position / 8;
            uint bitNumber = position % 8;
            byte value = _memory.Read(byteNumber);
            value |= MaskAlloc[bitNumber];
            _memory.Write(byteNumber, value);
        }

        private void FreeBlock(uint position)
        {
            uint byteNumber = position / 8;
            uint bitNumber = position % 8;
            byte value = _memory.Read(byteNumber);
            value &= MaskFree[bitNumber];
            _memory.Write(byteNumber, value);
        }
    }
}
