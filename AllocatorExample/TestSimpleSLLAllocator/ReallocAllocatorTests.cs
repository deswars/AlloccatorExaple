using AllocatorInterface;
using MemoryModel;
using Xunit;

namespace Allocators.SimpleSLLAllocator.Tests
{
    public class ReallocAllocatorTests
    {
        const uint size = 200;
        readonly Memory memory;
        readonly ReallocAllocator allocator;

        public ReallocAllocatorTests()
        {
            memory = new Memory(size);
            allocator = new ReallocAllocator(memory);
        }

        //Same Size
        [Fact]
        public void ReallocTestSame()
        {
            uint size = 12;
            uint addr = allocator.Alloc(size);
            uint newAddr = allocator.Realloc(addr, size);

            Assert.True(addr == newAddr);
        }

        //Smaller size: CF -> C-F+
        [Fact]
        public void ReallocTestSmaller1()
        {
            uint size = 12;
            uint smallSize = 8; 
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint addr4 = allocator.Alloc(size);
            allocator.Free(addr3);
            uint newAddr = allocator.Realloc(addr2, smallSize);

            Assert.True(newAddr == addr2);
            Header header = Header.ReadDataHeader(memory, newAddr);
            Assert.Equal(Header.HeaderSize + size, header.Address);
            Assert.Equal(smallSize, header.Size);
            Assert.Equal(MemoryStatus.Busy, header.Status);

            Header nextHeader = Header.Read(memory, header.NextAddress);
            Assert.Equal(2 * Header.HeaderSize + size + smallSize, nextHeader.Address);
            Assert.Equal(2 * size - smallSize, nextHeader.Size);
            Assert.Equal(MemoryStatus.Free, nextHeader.Status);
            Assert.Equal(addr4, nextHeader.NextAddress + Header.HeaderSize);
        }

        //Smaller size: CB -> C-FB
        [Fact]
        public void ReallocTestSmaller2()
        {
            uint size = 12;
            uint smallSize = 8;
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint newAddr = allocator.Realloc(addr2, smallSize);

            Assert.True(newAddr == addr2);
            Header header = Header.ReadDataHeader(memory, newAddr);
            Assert.Equal(Header.HeaderSize + size, header.Address);
            Assert.Equal(smallSize, header.Size);
            Assert.Equal(MemoryStatus.Busy, header.Status);

            Header nextHeader = Header.Read(memory, header.NextAddress);
            Assert.Equal(2 * Header.HeaderSize + size + smallSize, nextHeader.Address);
            Assert.Equal(size - smallSize - Header.HeaderSize, nextHeader.Size);
            Assert.Equal(MemoryStatus.Free, nextHeader.Status);
            Assert.Equal(addr3, nextHeader.NextAddress + Header.HeaderSize);
        }

        //Bigger Size: CF -> C+
        [Fact]
        public void ReallocTestBigger1()
        {
            uint size = 12;
            uint bigSize = size * 2 + Header.HeaderSize;
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint addr4 = allocator.Alloc(size);
            allocator.Free(addr3);
            uint newAddr = allocator.Realloc(addr2, bigSize);

            Assert.True(newAddr == addr2);
            Header header = Header.ReadDataHeader(memory, newAddr);
            Assert.Equal(Header.HeaderSize + size, header.Address);
            Assert.Equal(bigSize, header.Size);
            Assert.Equal(MemoryStatus.Busy, header.Status);

            Header nextHeader = Header.Read(memory, header.NextAddress);
            Assert.Equal(addr4, nextHeader.Address + Header.HeaderSize);
            Assert.Equal(size, nextHeader.Size);
            Assert.Equal(MemoryStatus.Busy, nextHeader.Status);
        }

        //Bigger Size: CF -> C+F-
        [Fact]
        public void ReallocTestBigger2()
        {
            uint size = 12;
            uint bigSize = size * 2;
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint addr4 = allocator.Alloc(size);
            allocator.Free(addr3);
            uint newAddr = allocator.Realloc(addr2, bigSize);

            Assert.True(newAddr == addr2);
            Header header = Header.ReadDataHeader(memory, newAddr);
            Assert.Equal(Header.HeaderSize + size, header.Address);
            Assert.Equal(bigSize, header.Size);
            Assert.Equal(MemoryStatus.Busy, header.Status);

            Header nextHeader = Header.Read(memory, header.NextAddress);
            Assert.Equal(size + bigSize + 2 * Header.HeaderSize, nextHeader.Address);
            Assert.Equal(addr4, nextHeader.NextAddress + Header.HeaderSize);
            Assert.Equal(0u, nextHeader.Size);
            Assert.Equal(MemoryStatus.Free, nextHeader.Status);
        }

        //Bigger Size: FCF -> C+
        [Fact]
        public void ReallocTestBigger3()
        {
            uint size = 12;
            uint bigSize = size * 3 + 2 * Header.HeaderSize;
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint addr4 = allocator.Alloc(size);
            uint addr5 = allocator.Alloc(size);
            allocator.Free(addr2);
            allocator.Free(addr4);
            uint newAddr = allocator.Realloc(addr3, bigSize);

            Assert.True(newAddr == addr2);
            Header header = Header.ReadDataHeader(memory, newAddr);
            Assert.Equal(Header.HeaderSize + size, header.Address);
            Assert.Equal(bigSize, header.Size);
            Assert.Equal(MemoryStatus.Busy, header.Status);

            Header nextHeader = Header.Read(memory, header.NextAddress);
            Assert.Equal(addr5, nextHeader.Address + Header.HeaderSize);
            Assert.Equal(size, nextHeader.Size);
            Assert.Equal(MemoryStatus.Busy, nextHeader.Status);
        }

        //Bigger Size: FCF -> C+F
        [Fact]
        public void ReallocTestBigger4()
        {
            uint size = 12;
            uint bigSize = size * 3 + Header.HeaderSize;
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint addr4 = allocator.Alloc(size);
            uint addr5 = allocator.Alloc(size);
            allocator.Free(addr2);
            allocator.Free(addr4);
            uint newAddr = allocator.Realloc(addr3, bigSize);

            Assert.True(newAddr == addr2);
            Header header = Header.ReadDataHeader(memory, newAddr);
            Assert.Equal(Header.HeaderSize + size, header.Address);
            Assert.Equal(bigSize, header.Size);
            Assert.Equal(MemoryStatus.Busy, header.Status);

            Header nextHeader = Header.Read(memory, header.NextAddress);
            Assert.Equal(size + bigSize + 2 * Header.HeaderSize, nextHeader.Address);
            Assert.Equal(0u, nextHeader.Size);
            Assert.Equal(MemoryStatus.Free, nextHeader.Status);

            Header nextNextHeader = Header.Read(memory, nextHeader.NextAddress);
            Assert.Equal(addr5, nextNextHeader.Address + Header.HeaderSize);
            Assert.Equal(size, nextNextHeader.Size);
            Assert.Equal(MemoryStatus.Busy, nextNextHeader.Status);
        }

        //Bigger Size: FCB -> C+B
        [Fact]
        public void ReallocTestBigger5()
        {
            uint size = 12;
            uint bigSize = size * 2 + Header.HeaderSize;
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint addr4 = allocator.Alloc(size);
            allocator.Free(addr2);
            uint newAddr = allocator.Realloc(addr3, bigSize);

            Assert.True(newAddr == addr2);
            Header header = Header.ReadDataHeader(memory, newAddr);
            Assert.Equal(Header.HeaderSize + size, header.Address);
            Assert.Equal(bigSize, header.Size);
            Assert.Equal(MemoryStatus.Busy, header.Status);

            Header nextHeader = Header.Read(memory, header.NextAddress);
            Assert.Equal(addr4, nextHeader.Address + Header.HeaderSize);
            Assert.Equal(size, nextHeader.Size);
            Assert.Equal(MemoryStatus.Busy, nextHeader.Status);
        }

        //Bigger Size: FCB -> F-C+B
        [Fact]
        public void ReallocTestBigger6()
        {
            uint size = 12;
            uint bigSize = size * 2;
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint addr4 = allocator.Alloc(size);
            allocator.Free(addr2);
            uint newAddr = allocator.Realloc(addr3, bigSize);

            Assert.True(newAddr == addr2 + Header.HeaderSize);
            Header header = Header.ReadDataHeader(memory, newAddr);
            Assert.Equal(2 * Header.HeaderSize + size, header.Address);
            Assert.Equal(bigSize, header.Size);
            Assert.Equal(MemoryStatus.Busy, header.Status);

            Header nextHeader = Header.Read(memory, header.NextAddress);
            Assert.Equal(addr4, nextHeader.Address + Header.HeaderSize);
            Assert.Equal(size, nextHeader.Size);
            Assert.Equal(MemoryStatus.Busy, nextHeader.Status);

            Header prevHeader = Header.ReadDataHeader(memory, addr2);
            Assert.Equal(addr2, prevHeader.Address + Header.HeaderSize);
            Assert.Equal(0u, prevHeader.Size);
            Assert.Equal(MemoryStatus.Free, prevHeader.Status);
        }

        //Bigger Size: FCF new Alloc
        [Fact]
        public void ReallocTestBigger7()
        {
            uint size = 12;
            uint bigSize = size * 3 + 3 * Header.HeaderSize;
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint addr4 = allocator.Alloc(size);
            uint addr5 = allocator.Alloc(size);
            allocator.Free(addr2);
            allocator.Free(addr4);
            uint newAddr = allocator.Realloc(addr3, bigSize);

            Assert.True(newAddr == addr5 + size + Header.HeaderSize);
        }

        //Bigger Size: BCF new Alloc
        [Fact]
        public void ReallocTestBigger8()
        {
            uint size = 12;
            uint bigSize = size * 2 + 2 * Header.HeaderSize;
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint addr4 = allocator.Alloc(size);
            uint addr5 = allocator.Alloc(size);
            allocator.Free(addr4);
            uint newAddr = allocator.Realloc(addr3, bigSize);

            Assert.True(newAddr == addr5 + size + Header.HeaderSize);
        }

        //Bigger Size: FCB new Alloc
        [Fact]
        public void ReallocTestBigger9()
        {
            uint size = 12;
            uint bigSize = size * 2 + 2 * Header.HeaderSize;
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint addr4 = allocator.Alloc(size);
            uint addr5 = allocator.Alloc(size);
            allocator.Free(addr2);
            uint newAddr = allocator.Realloc(addr3, bigSize);

            Assert.True(newAddr == addr5 + size + Header.HeaderSize);
        }

        //Bigger Size: BCB new Alloc
        [Fact]
        public void ReallocTestBigger10()
        {
            uint size = 12;
            uint bigSize = size + Header.HeaderSize;
            uint addr1 = allocator.Alloc(size);
            uint addr2 = allocator.Alloc(size);
            uint addr3 = allocator.Alloc(size);
            uint addr4 = allocator.Alloc(size);
            uint addr5 = allocator.Alloc(size);
            uint newAddr = allocator.Realloc(addr3, bigSize);

            Assert.True(newAddr == addr5 + size + Header.HeaderSize);
        }
    }
}
