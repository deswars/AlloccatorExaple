using AllocatorInterface;
using MemoryModel;
using Xunit;

namespace Allocators.SinglyLinkedListAllocator.Tests
{
    public class NaiveRellocableAllocatorTests
    {
        const uint size = 100;
        const uint addressSize = sizeof(uint);
        const uint headerSize = addressSize * 2;
        const uint statusMask = addressSize - 1;
        readonly Memory memory;
        readonly NaiveRellocableAllocator allocator;

        public NaiveRellocableAllocatorTests()
        {
            memory = new Memory(size);
            allocator = new NaiveRellocableAllocator(memory);
        }

        [Fact()]
        public void RellocTest()
        {
            uint blockSize1 = 20;
            uint blockSize2 = 40;
            uint addr1 = allocator.Alloc(blockSize1);
            uint addr2 = allocator.Alloc(blockSize1);

            uint addr3 = allocator.Relloc(addr1, blockSize2);
            Assert.Equal(allocator.Null, addr3);

            addr3 = allocator.Relloc(addr1, blockSize1);
            Assert.Equal(addr2 + blockSize1 + headerSize, addr3);
            uint firstBlockMixed = memory.ReadWord(addressSize);
            MemoryStatus firstBlockStatus = (MemoryStatus)(firstBlockMixed & statusMask);
            Assert.Equal(MemoryStatus.Free, firstBlockStatus);

            allocator.Free(addr2);
            allocator.Free(addr3);
            uint firstBlockNextAddr = memory.ReadWord(0);
            Assert.Equal(size - headerSize, firstBlockNextAddr);
        }
    }
}