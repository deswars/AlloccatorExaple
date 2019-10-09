using Xunit;
using MemoryModel;
using AllocatorInterface;

namespace Allocators.DLLAllocator.Tests
{
    public class AllocatorTests
    {
        const uint size = 100;
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
            Header firstHeader = ReadHeader(0);
            Assert.Equal(size - Header.Size, firstHeader.NextAddress);
            Assert.Equal(allocator.Null, firstHeader.PrevAddress);
            Assert.Equal(size - 2 * Header.Size, firstHeader.Mixed);

            Header lastHeader = ReadHeader(size - Header.Size);
            Assert.Equal(allocator.Null, lastHeader.NextAddress);
            Assert.Equal(0u, lastHeader.PrevAddress);
            Assert.Equal((uint)MemoryStatus.System, lastHeader.Mixed);
        }

        [Fact]
        public void AllocTest()
        {
            //first block
            uint block1Size = 10;
            uint block1RealSize = (block1Size - 1 + Header.AddressSize) & Header.SizeMask;
            uint block1Address = allocator.Alloc(block1Size);
            Assert.Equal(Header.Size, block1Address);

            Header header1 = ReadHeader(0);
            Assert.Equal(Header.Size + block1RealSize, header1.NextAddress);
            Assert.Equal(allocator.Null, header1.PrevAddress);
            Assert.Equal(block1RealSize | (uint)MemoryStatus.Busy, header1.Mixed);

            //second block
            uint block2Size = 20;
            uint block2RealSize = (block2Size - 1 + Header.AddressSize) & Header.SizeMask;
            uint block2Address = allocator.Alloc(block2Size);
            Assert.Equal(header1.NextAddress + Header.Size, block2Address);

            Header header2 = ReadHeader(header1.NextAddress);
            Assert.Equal(header1.NextAddress + Header.Size + block2RealSize, header2.NextAddress);
            Assert.Equal(block1Address - Header.Size,header2.PrevAddress);
            Assert.Equal(block2RealSize | (uint)MemoryStatus.Busy, header2.Mixed);

            //check free memory
            Header headerFree = ReadHeader(header2.NextAddress);
            uint freeBlockSize = size - block1RealSize - block2RealSize - 4 * Header.Size;
            Assert.Equal(size - Header.Size, headerFree.NextAddress);
            Assert.Equal(block2Address - Header.Size, headerFree.PrevAddress);
            Assert.Equal(freeBlockSize | (uint)MemoryStatus.Free, headerFree.Mixed);

            //too big block
            uint BigBlockSize = freeBlockSize + 1;
            uint BigBlockAddress = allocator.Alloc(BigBlockSize);
            Assert.Equal(allocator.Null, BigBlockAddress);
            Assert.Equal(Header.Size + block1RealSize, header1.NextAddress);
            Assert.Equal(header1.NextAddress + Header.Size + block2RealSize, header2.NextAddress);
            Assert.Equal(size - Header.Size, headerFree.NextAddress);

            //isufficient space for new header
            uint block3Size = freeBlockSize - Header.AddressSize;
            uint block3Address = allocator.Alloc(block3Size);
            Assert.Equal(header2.NextAddress + Header.Size, block3Address);

            Header header3 = ReadHeader(header2.NextAddress);
            Assert.Equal(size - Header.Size, header3.NextAddress);
            Assert.Equal(freeBlockSize | (uint)MemoryStatus.Busy, header3.Mixed);
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

            uint block1Header = block1Address - Header.Size;
            uint block2Header = block2Address - Header.Size;
            uint block3Header = block3Address - Header.Size;
            uint block4Header = block4Address - Header.Size;
            uint block5Header = block5Address - Header.Size;

            //free 1 block
            allocator.Free(block3Address);
            uint freeAddress = block3Header;
            Header headerFree = ReadHeader(freeAddress);
            Assert.Equal(block4Header, headerFree.NextAddress);
            uint freeSize = block4Header - block3Header - Header.Size;
            Assert.Equal(freeSize | (uint)MemoryStatus.Free, headerFree.Mixed);
            Header header4 = ReadHeader(block4Header);
            Assert.Equal(block3Header, header4.PrevAddress);

            //free after freeBlock
            allocator.Free(block4Address);
            headerFree = ReadHeader(freeAddress);
            Assert.Equal(block5Header, headerFree.NextAddress);
            freeSize = block5Header - block3Header - Header.Size;
            Assert.Equal(freeSize | (uint)MemoryStatus.Free, headerFree.Mixed);
            Header header5 = ReadHeader(block5Header);
            Assert.Equal(block3Header, header5.PrevAddress);

            //free before freeBlock
            allocator.Free(block2Address);
            freeAddress = block2Header;
            headerFree = ReadHeader(freeAddress);
            Assert.Equal(block5Header, headerFree.NextAddress);
            freeSize = block5Header - block2Header - Header.Size;
            Assert.Equal(freeSize | (uint)MemoryStatus.Free, headerFree.Mixed);
            header5 = ReadHeader(block5Header);
            Assert.Equal(block2Header, header5.PrevAddress);

            //free all
            allocator.Free(block1Address);
            allocator.Free(block5Address);
            freeAddress = block1Header;
            headerFree = ReadHeader(freeAddress);
            Assert.Equal(size - Header.Size, headerFree.NextAddress);
            freeSize = size - 2 * Header.Size;
            Assert.Equal(freeSize | (uint)MemoryStatus.Free, headerFree.Mixed);
            Header headerLast = ReadHeader(size - Header.Size);
            Assert.Equal(0u, headerLast.PrevAddress);
        }

        protected Header ReadHeader(uint address)
        {
            uint next = memory.ReadWord(address);
            uint prev = memory.ReadWord(address + Header.AddressSize);
            uint mixed = memory.ReadWord(address + 2 * Header.AddressSize);
            Header result;
            result.NextAddress = next;
            result.PrevAddress = prev;
            result.Mixed = mixed;
            return result;
        }
    }
}