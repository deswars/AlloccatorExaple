using AllocatorInterface;
using MemoryModel;
using Xunit;

namespace Allocators.SimpleDLLAllocator.Tests
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
            Header firstExpected;
            firstExpected.Address = 0;
            firstExpected.Size = size - 2 * Header.HeaderSize;
            firstExpected.Status = MemoryStatus.Free;
            firstExpected.PrevAddress = allocator.Null;

            Header lastExpected;
            lastExpected.Address = size - Header.HeaderSize;
            lastExpected.Size = 0;
            lastExpected.Status = MemoryStatus.System;
            lastExpected.PrevAddress = 0;

            Header firstHeader = Header.Read(memory, 0);
            Header lastHeader = Header.Read(memory, firstExpected.NextAddress);

            Assert.Equal(firstExpected, firstHeader);
            Assert.Equal(lastExpected, lastHeader);
        }

        [Fact]
        public void AllocTest()
        {
            uint smallSize = 10;
            uint realSmallSize = 12;
            uint bigSize = 20;

            uint firstExpected = Header.HeaderSize;
            uint first = allocator.Alloc(smallSize);
            Assert.Equal(firstExpected, first);

            uint secondExpected = realSmallSize + 2 * Header.HeaderSize;
            uint second = allocator.Alloc(bigSize);
            Assert.Equal(secondExpected, second);

            //not enought free memory
            uint freeSpace = size - 4 * Header.HeaderSize - realSmallSize - bigSize;
            uint tooLarge = allocator.Alloc(freeSpace + 1);
            Assert.Equal(allocator.Null, tooLarge);

            //not enought for free block
            uint thirdExpected = realSmallSize + bigSize + 3 * Header.HeaderSize;
            uint third = allocator.Alloc(freeSpace - 1);
            Assert.Equal(thirdExpected, third);

            //Test Memory Structure
            Header firstHeader = Header.Read(memory, 0);
            Header firstHeaderExpected;
            firstHeaderExpected.Address = 0;
            firstHeaderExpected.Size = realSmallSize;
            firstHeaderExpected.Status = MemoryStatus.Busy;
            firstHeaderExpected.PrevAddress = allocator.Null;
            Assert.Equal(firstHeaderExpected, firstHeader);

            Header secondHeader = Header.Read(memory, firstHeader.NextAddress);
            Header secondHeaderExpected;
            secondHeaderExpected.Address = realSmallSize + Header.HeaderSize;
            secondHeaderExpected.Size = bigSize;
            secondHeaderExpected.Status = MemoryStatus.Busy;
            secondHeaderExpected.PrevAddress = firstHeaderExpected.Address;
            Assert.Equal(secondHeaderExpected, secondHeader);

            Header thirdHeader = Header.Read(memory, secondHeader.NextAddress);
            Header thirdHeaderExpected;
            thirdHeaderExpected.Address = realSmallSize + bigSize + 2 * Header.HeaderSize;
            thirdHeaderExpected.Size = freeSpace;
            thirdHeaderExpected.Status = MemoryStatus.Busy;
            thirdHeaderExpected.PrevAddress = secondHeaderExpected.Address;
            Assert.Equal(thirdHeaderExpected, thirdHeader);

            Header lastHeader = Header.Read(memory, thirdHeader.NextAddress);
            Header lastHeaderExpected;
            lastHeaderExpected.Address = size - Header.HeaderSize;
            lastHeaderExpected.Size = 0;
            lastHeaderExpected.Status = MemoryStatus.System;
            lastHeaderExpected.PrevAddress = thirdHeaderExpected.Address;
            Assert.Equal(lastHeaderExpected, lastHeader);
        }

        [Fact]
        public void FreeTest()
        {
            uint blockSize = 8;
            uint block1Address = allocator.Alloc(blockSize);
            uint block2Address = allocator.Alloc(blockSize);
            uint block3Address = allocator.Alloc(blockSize);
            uint block4Address = allocator.Alloc(blockSize);
            uint block5Address = allocator.Alloc(blockSize);

            //free 1 block
            allocator.Free(block3Address);
            Header freeHeader = Header.ReadDataHeader(memory, block3Address);

            Header freeExpected;
            freeExpected.Address = 2 * blockSize + 2 * Header.HeaderSize;
            freeExpected.Size = blockSize;
            freeExpected.Status = MemoryStatus.Free;
            freeExpected.PrevAddress = blockSize + Header.HeaderSize;
            Assert.Equal(freeExpected, freeHeader);

            //free after freeBlock
            allocator.Free(block4Address);
            freeHeader = Header.ReadDataHeader(memory, block3Address);

            freeExpected.Address = 2 * blockSize + 2 * Header.HeaderSize;
            freeExpected.Size = 2 * blockSize + Header.HeaderSize;
            freeExpected.Status = MemoryStatus.Free;
            freeExpected.PrevAddress = blockSize + Header.HeaderSize;
            Assert.Equal(freeExpected, freeHeader);

            //free before freeBlock
            allocator.Free(block2Address);
            freeHeader = Header.ReadDataHeader(memory, block2Address);

            freeExpected.Address = blockSize + Header.HeaderSize;
            freeExpected.Size = 3 * blockSize + 2 * Header.HeaderSize;
            freeExpected.Status = MemoryStatus.Free;
            freeExpected.PrevAddress = 0;
            Assert.Equal(freeExpected, freeHeader);

            //free all
            allocator.Free(block1Address);
            allocator.Free(block5Address);

            Header firstHeader = Header.Read(memory, 0);

            Header firstExpected;
            firstExpected.Address = 0;
            firstExpected.Size = size - 2 * Header.HeaderSize;
            firstExpected.Status = MemoryStatus.Free;
            firstExpected.PrevAddress = allocator.Null;
            Assert.Equal(firstExpected, firstHeader);

            Header lastHeader = Header.Read(memory, firstExpected.NextAddress);

            Header lastExpected;
            lastExpected.Address = size - Header.HeaderSize;
            lastExpected.Size = 0;
            lastExpected.Status = MemoryStatus.System;
            lastExpected.PrevAddress = firstExpected.Address;
            Assert.Equal(lastExpected, lastHeader);
        }
    }
}
