using AllocatorInterface;
using Allocators.SinglyLinkedListAllocator;
using MemoryModel;
using Xunit;

namespace TestSinglyLinkedListAllocator
{
    public class TestAllocator
    {
        const uint size = 100;
        const uint addressSize = sizeof(uint);
        const uint headerSize = addressSize * 2;
        const uint statusMask = addressSize - 1;
        const uint sizeMask = ~statusMask;
        readonly Memory memory;
        readonly Allocator allocator;

        public TestAllocator()
        {
            memory = new Memory(size);
            allocator = new Allocator(memory);
        }

        [Fact]
        public void TestBuild()
        {
            uint Block1NextAddress = memory.ReadWord(0);
            uint Block1Mixed = memory.ReadWord(addressSize);
            Assert.Equal(size - headerSize, Block1NextAddress);
            Assert.Equal(size - 2 * headerSize, Block1Mixed);

            uint Block2NextAddress = memory.ReadWord(size - headerSize);
            uint Block2Mixed = memory.ReadWord(size - headerSize + addressSize);
            Assert.Equal(allocator.Null, Block2NextAddress);
            Assert.Equal((uint)MemoryStatus.System, Block2Mixed);
        }
        [Fact]
        public void TestAlloc()
        {
            //first block
            uint block1Size = 10;
            uint block1RealSize = (block1Size - 1 + addressSize) & sizeMask;
            uint block1Address = allocator.Alloc(block1Size);
            Assert.Equal(headerSize, block1Address);

            uint block1NextAddress = memory.ReadWord(0);
            uint block1Mixed = memory.ReadWord(addressSize);
            Assert.Equal(headerSize + block1RealSize, block1NextAddress);
            Assert.Equal(block1RealSize | (uint)MemoryStatus.Busy, block1Mixed);

            //second block
            uint block2Size = 20;
            uint block2RealSize = (block2Size - 1 + addressSize) & sizeMask;
            uint block2Address = allocator.Alloc(block2Size);
            Assert.Equal(block1NextAddress + headerSize, block2Address);

            uint block2NextAddress = memory.ReadWord(block1NextAddress);
            uint block2Mixed = memory.ReadWord(block1NextAddress + addressSize);
            Assert.Equal(block1NextAddress + headerSize + block2RealSize, block2NextAddress);
            Assert.Equal(block2RealSize | (uint)MemoryStatus.Busy, block2Mixed);

            //check free memory
            uint freeBlockNextAddress = memory.ReadWord(block2NextAddress);
            uint freeBlockMixed = memory.ReadWord(block2NextAddress + addressSize);
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
            uint block3Mixed = memory.ReadWord(block2NextAddress + addressSize);
            Assert.Equal(size - headerSize, block3NextAddress);
            Assert.Equal(freeBlockSize | (uint)MemoryStatus.Busy, block3Mixed);
        }
    }
}
