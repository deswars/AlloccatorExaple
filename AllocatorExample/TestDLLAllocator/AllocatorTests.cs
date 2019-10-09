using Xunit;
using MemoryModel;
using AllocatorInterface;

namespace Allocators.DoubleLinkedListAllocator.Tests
{
    public class AllocatorTests
    {
        const uint size = 100;
        const uint addressSize = sizeof(uint);
        const uint headerSize = addressSize * 3;
        const uint statusMask = addressSize - 1;
        const uint sizeMask = ~statusMask;
        readonly Memory memory;
        readonly Allocator allocator;

        public AllocatorTests()
        {
            memory = new Memory(size);
            allocator = new Allocator(memory);
        }

        [Fact]
        public void BuildTest()
        {
            uint Block1NextAddress = memory.ReadWord(0);
            uint Block1PrevAddress = memory.ReadWord(addressSize);
            uint Block1Mixed = memory.ReadWord(2 * addressSize);
            Assert.Equal(size - headerSize, Block1NextAddress);
            Assert.Equal(allocator.Null, Block1PrevAddress);
            Assert.Equal(size - 2 * headerSize, Block1Mixed);

            uint Block2NextAddress = memory.ReadWord(size - headerSize);
            uint Block2PrevAddress = memory.ReadWord(size - headerSize + addressSize);
            uint Block2Mixed = memory.ReadWord(size - headerSize + 2 * addressSize);
            Assert.Equal(allocator.Null, Block2NextAddress);
            Assert.Equal(0u, Block2PrevAddress);
            Assert.Equal((uint)MemoryStatus.System, Block2Mixed);
        }

        [Fact]
        public void AllocTest()
        {
            //first block
            uint block1Size = 10;
            uint block1RealSize = (block1Size - 1 + addressSize) & sizeMask;
            uint block1Address = allocator.Alloc(block1Size);
            Assert.Equal(headerSize, block1Address);

            uint block1NextAddress = memory.ReadWord(0);
            uint block1Mixed = memory.ReadWord(2 * addressSize);
            Assert.Equal(headerSize + block1RealSize, block1NextAddress);
            Assert.Equal(block1RealSize | (uint)MemoryStatus.Busy, block1Mixed);

            //second block
            uint block2Size = 20;
            uint block2RealSize = (block2Size - 1 + addressSize) & sizeMask;
            uint block2Address = allocator.Alloc(block2Size);
            Assert.Equal(block1NextAddress + headerSize, block2Address);

            uint block2NextAddress = memory.ReadWord(block1NextAddress);
            uint block2Mixed = memory.ReadWord(block1NextAddress + 2 * addressSize);
            Assert.Equal(block1NextAddress + headerSize + block2RealSize, block2NextAddress);
            Assert.Equal(block2RealSize | (uint)MemoryStatus.Busy, block2Mixed);

            //check free memory
            uint freeBlockNextAddress = memory.ReadWord(block2NextAddress);
            uint freeBlockMixed = memory.ReadWord(block2NextAddress + 2 * addressSize);
            uint freeBlockSize = size - block1RealSize - block2RealSize - 4 * headerSize;
            Assert.Equal(size - headerSize, freeBlockNextAddress);
            Assert.Equal(freeBlockSize | (uint)MemoryStatus.Free, freeBlockMixed);

            //too big block
            uint BigBlockSize = freeBlockSize + 1;
            uint BigBlockAddress = allocator.Alloc(BigBlockSize);
            Assert.Equal(allocator.Null, BigBlockAddress);
            Assert.Equal(headerSize + block1RealSize, block1NextAddress);
            Assert.Equal(block1NextAddress + headerSize + block2RealSize, block2NextAddress);
            Assert.Equal(size - headerSize, freeBlockNextAddress);

            //isufficient space for new header
            uint block3Size = freeBlockSize - addressSize;
            uint block3Address = allocator.Alloc(block3Size);
            Assert.Equal(block2NextAddress + headerSize, block3Address);

            uint block3NextAddress = memory.ReadWord(block2NextAddress);
            uint block3Mixed = memory.ReadWord(block2NextAddress + 2 * addressSize);
            Assert.Equal(size - headerSize, block3NextAddress);
            Assert.Equal(freeBlockSize | (uint)MemoryStatus.Busy, block3Mixed);
        }

        [Fact]
        public void FreeTest()
        {
            uint blockSize = 4;
            uint block1Address = allocator.Alloc(blockSize);
            uint block2Address = allocator.Alloc(blockSize);
            uint block3Address = allocator.Alloc(blockSize);
            uint block4Address = allocator.Alloc(blockSize);
            uint block5Address = allocator.Alloc(blockSize);

            uint block1Header = block1Address - headerSize;
            uint block2Header = block2Address - headerSize;
            uint block3Header = block3Address - headerSize;
            uint block4Header = block4Address - headerSize;
            uint block5Header = block5Address - headerSize;

            //free 1 block
            allocator.Free(block3Address);
            uint freeAddress = block3Header;
            uint freeNext = memory.ReadWord(freeAddress);
            uint freeMixed = memory.ReadWord(freeAddress + 2 * addressSize);
            Assert.Equal(block4Header, freeNext);
            uint freeSize = block4Header - block3Header - headerSize;
            Assert.Equal(freeSize | (uint)MemoryStatus.Free, freeMixed);
            uint block4Prev = memory.ReadWord(block4Header + addressSize);
            Assert.Equal(block3Header, block4Prev);

            //free after freeBlock
            allocator.Free(block4Address);
            freeNext = memory.ReadWord(freeAddress);
            freeMixed = memory.ReadWord(freeAddress + 2 * addressSize);
            Assert.Equal(block5Header, freeNext);
            freeSize = block5Header - block3Header - headerSize;
            Assert.Equal(freeSize | (uint)MemoryStatus.Free, freeMixed);
            uint block5Prev = memory.ReadWord(block5Header + addressSize);
            Assert.Equal(block3Header, block5Prev);

            //free before freeBlock
            allocator.Free(block2Address);
            freeAddress = block2Header;
            freeNext = memory.ReadWord(freeAddress);
            freeMixed = memory.ReadWord(freeAddress + 2 * addressSize);
            Assert.Equal(block5Header, freeNext);
            freeSize = block5Header - block2Header - headerSize;
            Assert.Equal(freeSize | (uint)MemoryStatus.Free, freeMixed);
            block5Prev = memory.ReadWord(block5Header + addressSize);
            Assert.Equal(block2Header, block5Prev);

            //free all
            allocator.Free(block1Address);
            allocator.Free(block5Address);
            freeAddress = block1Header;
            freeNext = memory.ReadWord(freeAddress);
            freeMixed = memory.ReadWord(freeAddress + 2 * addressSize);
            Assert.Equal(size - headerSize, freeNext);
            freeSize = size - 2 * headerSize;
            Assert.Equal(freeSize | (uint)MemoryStatus.Free, freeMixed);
            uint lastPrev = memory.ReadWord(size - headerSize + addressSize);
            Assert.Equal(0u, lastPrev);
        }
    }
}