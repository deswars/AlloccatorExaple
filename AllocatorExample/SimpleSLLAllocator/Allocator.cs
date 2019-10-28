using AllocatorInterface;
using MemoryModel;
using System.Diagnostics.Contracts;

namespace Allocators.SimpleSLLAllocator
{
    public class Allocator : IAllocator
    {
        //All addresses are multile of addressSize
        //Block format: Header|Data
        //Header = Size & Status
        public Allocator(Memory memory)
        {
            Contract.Requires(memory != null);

            _memory = memory;
            uint size = _memory.GetSize();

            Header first;
            first.Address = 0;
            first.Size = size - 2 * Header.HeaderSize;
            first.Status = MemoryStatus.Free;
            first.Write(_memory);

            Header last;
            last.Address = size - Header.HeaderSize;
            last.Size = 0;
            last.Status = MemoryStatus.System;
            last.Write(_memory);
        }

        public uint Null
        {
            get
            {
                return uint.MaxValue;
            }
        }

        public uint Alloc(uint size)
        {
            uint currSize = RoundUpSize(size);
            Header currHeader = Header.Read(_memory, 0);

            while (currHeader.Status != MemoryStatus.System)
            {
                if ((currHeader.Status == MemoryStatus.Free) && (currHeader.Size >= currSize))
                {
                    Header allocated = AllocBlock(currHeader, currSize);
                    return allocated.DataAddress;
                }
                currHeader = Header.Read(_memory, currHeader.NextAddress);
            }
            return Null;
        }

        public void Free(uint address)
        {
            Header currHeader = Header.ReadDataHeader(_memory, address);
            currHeader.Status = MemoryStatus.Free;
            currHeader = TryUniteWithNext(currHeader);
            currHeader = TryUniteWithPrev(currHeader);
            currHeader.Write(_memory);
        }

#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly Memory _memory;
#pragma warning restore CA1051 // Do not declare visible instance fields

        protected static uint RoundUpSize(uint size)
        {
            return (size + Header.AddressSize - 1) & Header.SizeMask;
        }

        protected Header AllocBlock(Header currHeader, uint currSize)
        {
            if (currHeader.Size > currSize + Header.HeaderSize)
            {
                return SplitAllocBlock(currHeader, currSize);
            }

            currHeader.Status = MemoryStatus.Busy;
            currHeader.Write(_memory);
            return currHeader;
        }

        protected Header SplitAllocBlock(Header currHeader, uint currSize)
        {
            uint freeSize = currHeader.Size - currSize - Header.HeaderSize;
            currHeader.Size = currSize;
            currHeader.Status = MemoryStatus.Busy;
            currHeader.Write(_memory);

            Header freeHeader;
            freeHeader.Address = currHeader.NextAddress;
            freeHeader.Size = freeSize;
            freeHeader.Status = MemoryStatus.Free;
            freeHeader.Write(_memory);

            return currHeader;
        }

        protected Header TryUniteWithNext(Header currHeader)
        {
            Header nextHeader = Header.Read(_memory, currHeader.NextAddress);
            if (nextHeader.Status == MemoryStatus.Free)
            {
                currHeader = UniteWithNext(currHeader, nextHeader);
            }
            return currHeader;
        }

        protected Header TryUniteWithPrev(Header currHeader)
        {
            Header prevHeader = FindPrevOrFirst(currHeader);
            if (prevHeader == currHeader)
            {
                return currHeader;
            }

            if (prevHeader.Status == MemoryStatus.Free)
            {
                currHeader = UniteWithNext(prevHeader, currHeader);
            }
            return currHeader;
        }

        protected Header FindPrevOrFirst(Header currHeader)
        {
            if (currHeader.Address == 0)
            {
                return currHeader;
            }

            Header prevHeader = Header.Read(_memory, 0);
            while (prevHeader.NextAddress != currHeader.Address)
            {
                prevHeader = Header.Read(_memory, prevHeader.NextAddress);
            }

            return prevHeader;
        }

        protected static Header UniteWithNext(Header first, Header second)
        {
            first.Size += second.Size + Header.HeaderSize;
            return first;
        }
    }
}
