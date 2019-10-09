using AllocatorInterface;
using MemoryModel;

namespace Allocators.DLLAllocator
{
    public class Allocator : IAllocator
    {
        //All addresses are multile of addressSize
        //Block format: NextBlockAddress|PrevBlockAddress|Mixed|Data
        //Mixed = Size & Status

        public Allocator(Memory memory)
        {
            _memory = memory;
            uint size = _memory.GetSize();

            Header first;
            first.NextAddress = size - Header.Size;
            first.PrevAddress = Null;
            first.Mixed = Header.GenerateMixed(size - 2 * Header.Size, MemoryStatus.Free);
            WriteHeader(0, first);

            Header last;
            last.NextAddress = Null;
            last.PrevAddress = 0;
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
            uint prev = _memory.ReadWord(address + Header.AddressSize);
            uint mixed = _memory.ReadWord(address + 2 * Header.AddressSize);
            Header result;
            result.NextAddress = next;
            result.PrevAddress = prev;
            result.Mixed = mixed;
            return result;
        }

        protected void WriteHeader(uint address, Header header)
        {
            _memory.WriteWord(address, header.NextAddress);
            _memory.WriteWord(address + Header.AddressSize, header.PrevAddress);
            _memory.WriteWord(address + 2 * Header.AddressSize, header.Mixed);
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
            nextHeader.PrevAddress = address;
            nextHeader.Mixed = Header.GenerateMixed(freeSize, MemoryStatus.Free);
            WriteHeader(nextAddress, nextHeader);

            header.SetMixed(size, MemoryStatus.Busy);
            header.NextAddress = nextAddress;
            WriteHeader(address, header);

            Header nextNextHeader = ReadHeader(nextHeader.NextAddress);
            nextNextHeader.PrevAddress = nextAddress;
            WriteHeader(nextHeader.NextAddress, nextNextHeader);
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

                Header nextNextHeader = ReadHeader(header.NextAddress);
                nextNextHeader.PrevAddress = address;
                WriteHeader(header.NextAddress, nextNextHeader);
            }
        }

        protected void UniteWithPrevious(uint address)
        {
            Header header = ReadHeader(address);
            if (header.PrevAddress != Null)
            {
                Header prevHeader = ReadHeader(header.PrevAddress);
                if (prevHeader.GetStatus() == MemoryStatus.Free)
                {
                    uint size = header.GetSize();
                    uint prevSize = prevHeader.GetSize();
                    size += prevSize + Header.Size;
                    prevHeader.SetSize(size);
                    prevHeader.NextAddress = header.NextAddress;
                    WriteHeader(header.PrevAddress, prevHeader);

                    Header NextHeader = ReadHeader(prevHeader.NextAddress);
                    NextHeader.PrevAddress = header.PrevAddress;
                    WriteHeader(prevHeader.NextAddress, NextHeader);
                }
            }
        }
    }
}  