using AllocatorInterface;
using MemoryModel;
using Xunit;

namespace Allocators.SLLAllocator.Tests
{
    public class NaiveRellocableAllocatorTests
    {
        const uint size = 100;
        readonly Memory memory;
        readonly NaiveReallocableAllocator allocator;

        public NaiveRellocableAllocatorTests()
        {
            memory = new Memory(size);
            allocator = new NaiveReallocableAllocator(memory);
        }

        [Fact()]
        public void RellocTest()
        {
            uint blockSize1 = 20;
            uint blockSize2 = 40;
            uint addr1 = allocator.Alloc(blockSize1);
            uint addr2 = allocator.Alloc(blockSize1);

            uint addr3 = allocator.Realloc(addr1, blockSize2);
            Assert.Equal(allocator.Null, addr3);

            addr3 = allocator.Realloc(addr1, blockSize1);
            Assert.Equal(addr2 + blockSize1 + Header.Size, addr3);
            uint firstBlockMixed = memory.ReadWord(Header.AddressSize);
            MemoryStatus firstBlockStatus = (MemoryStatus)(firstBlockMixed & Header.StatusMask);
            Assert.Equal(MemoryStatus.Free, firstBlockStatus);

            allocator.Free(addr2);
            allocator.Free(addr3);
            uint firstBlockNextAddr = memory.ReadWord(0);
            Assert.Equal(size - Header.Size, firstBlockNextAddr);
        }
    }
}