using AllocatorInterface;
using MemoryModel;

namespace Allocators.SLLAllocator
{
    public class Allocator : IAllocator
    {
        //All addresses are multile of addressSize
        //Block format: NextBlockAddress|Mixed|Data
        //Mixed = Size & Status
        public Allocator(Memory memory)
        {
            _memory = memory;
            uint size = _memory.GetSize();

            Header first;
            first.NextAddress = size - Header.Size;
            first.Mixed = Header.GenerateMixed(size - 2 * Header.Size, MemoryStatus.Free);
            WriteHeader(0, first);

            Header last;
            last.NextAddress = Null;
            last.Mixed = Header.GenerateMixed(0, MemoryStatus.System);
            WriteHeader(first.NextAddress, last);
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
            //round up requested size
            uint requiredSize = (size + Header.AddressSize - 1) & Header.SizeMask;

            uint headerAddress = 0;
            Header header = ReadHeader(headerAddress);
            while (header.NextAddress != Null)
            {
                if (header.GetStatus() == MemoryStatus.Free)
                {
                    if (header.GetSize() >= requiredSize)
                    {
                        if (header.GetSize() <= requiredSize + Header.Size)
                        {
                            header.SetStatus(MemoryStatus.Busy);
                            WriteHeader(headerAddress, header);
                        }
                        else
                        {
                            SplitAndAllocBlock(headerAddress, requiredSize);
                        }
                        return GetBlockDataAddress(headerAddress);
                    }
                }
                headerAddress = header.NextAddress;
                header = ReadHeader(headerAddress);
            }
            return Null;
        }

        public void Free(uint address)
        {
            uint headerAddress = GetBlockHeaderAddress(address);
            Header header = ReadHeader(headerAddress);
            header.SetStatus(MemoryStatus.Free);
            WriteHeader(headerAddress, header);

            UniteWithNext(headerAddress);
            UniteWithPrevious(headerAddress);
        }

        protected Memory _memory;

        protected Header ReadHeader(uint address)
        {
            uint next = _memory.ReadWord(address);
            uint mixed = _memory.ReadWord(address + Header.AddressSize);
            Header result;
            result.NextAddress = next;
            result.Mixed = mixed;
            return result;
        }

        protected void WriteHeader(uint address, Header header)
        {
            _memory.WriteWord(address, header.NextAddress);
            _memory.WriteWord(address + Header.AddressSize, header.Mixed);
        }

        protected uint GetBlockDataAddress(uint address)
        {
            return address + Header.Size;
        }

        protected uint GetBlockHeaderAddress(uint dataAddress)
        {
            return dataAddress - Header.Size;
        }

        protected void SplitAndAllocBlock(uint address, uint size)
        {
            Header header = ReadHeader(address);
            uint blockSize = header.GetSize();
            uint freeSize = blockSize - size - Header.Size;

            uint nextAddress = address + Header.Size + size;
            Header nextHeader;
            nextHeader.NextAddress = header.NextAddress;
            nextHeader.Mixed = Header.GenerateMixed(freeSize, MemoryStatus.Free);
            WriteHeader(nextAddress, nextHeader);

            header.SetMixed(size, MemoryStatus.Busy);
            header.NextAddress = nextAddress;
            WriteHeader(address, header);
        }

        protected void UniteWithNext(uint address)
        {
            Header header = ReadHeader(address);
            Header nextHeader = ReadHeader(header.NextAddress);
            if (nextHeader.GetStatus() == MemoryStatus.Free)
            {
                uint size = header.GetSize();
                uint nextSize = nextHeader.GetSize();
                size += nextSize + Header.Size;
                header.SetSize(size);
                header.NextAddress = nextHeader.NextAddress;
                WriteHeader(address, header);
            }
        }

        protected void UniteWithPrevious(uint address)
        {
            uint prevAddress = 0;
            uint currentAddress = 0;
            while (currentAddress != address)
            {
                prevAddress = currentAddress;
                Header header = ReadHeader(currentAddress);
                currentAddress = header.NextAddress;
            }
            if (prevAddress != currentAddress)
            {
                Header prevHeader = ReadHeader(prevAddress);
                if (prevHeader.GetStatus() == MemoryStatus.Free)
                {
                    Header currentHeader = ReadHeader(currentAddress);
                    uint prevSize = prevHeader.GetSize();
                    prevSize += currentHeader.GetSize() + Header.Size;
                    prevHeader.SetSize(prevSize);
                    prevHeader.NextAddress = currentHeader.NextAddress;
                    WriteHeader(prevAddress, prevHeader);
                }
            }
        }
    }
}
