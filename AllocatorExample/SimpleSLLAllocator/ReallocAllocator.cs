using AllocatorInterface;
using MemoryModel;

namespace Allocators.SimpleSLLAllocator
{
    //Tries to resize block if impossible, then alloc new block and free old
    public class ReallocAllocator : Allocator, IAllocatorReallocable
    {
        public ReallocAllocator(Memory memory) : base(memory)
        {
        }

        public uint Realloc(uint address, uint newSize)
        {
            uint currSize = RoundUpSize(newSize);
            Header header = Header.ReadDataHeader(_memory, address);

            if (currSize == header.Size)
            {
                return address;
            }
            if (currSize < header.Size)
            {
                return ReallocSmaller(header, currSize);
            }
            return ReallocBigger(header, currSize);
        }

        private uint ReallocSmaller(Header header, uint newSize)
        {
            Header nextHeader = Header.Read(_memory, header.NextAddress);
            if (nextHeader.Status == MemoryStatus.Free)
            {
                return ReallocSmallerFree(header, nextHeader, newSize);
            }
            else
            {
                return ReallocSmallerBusy(header, newSize);
            }
        }

        private uint ReallocSmallerFree(Header header, Header nextHeader, uint newSize)
        {
            //resize block whith next
            uint delta = header.Size - newSize;
            header.Size = newSize;
            header.Write(_memory);
            nextHeader.Size += delta;
            nextHeader.Address = header.NextAddress;
            nextHeader.Write(_memory);

            return header.DataAddress;
        }

        private uint ReallocSmallerBusy(Header header, uint newSize)
        {
            //create new free block
            uint delta = header.Size - newSize;
            header.Size = newSize;
            header.Write(_memory);

            Header freeHeader;
            freeHeader.Address = header.NextAddress;
            freeHeader.Size = delta - Header.HeaderSize;
            freeHeader.Status = MemoryStatus.Free;
            freeHeader.Write(_memory);

            return header.DataAddress;
        }

        private uint ReallocBigger(Header header, uint newSize)
        {
            Header nextHeader = Header.Read(_memory, header.NextAddress);
            if (nextHeader.Status == MemoryStatus.Free)
            {
                return ReallocBiggerCurrFree(header, nextHeader, newSize);
            }
            else
            {
                return ReallocBiggerCurrBusy(header, newSize);
            }
        }

        private uint ReallocBiggerCurrFree(Header header, Header nextHeader, uint newSize)
        {
            if (newSize <= header.Size + nextHeader.Size + Header.HeaderSize)
            {
                if (newSize > header.Size + nextHeader.Size)
                {
                    //relloc require all space of both blocks
                    header.Size += nextHeader.Size + Header.HeaderSize;
                    header.Write(_memory);
                    return header.DataAddress;
                }
                else
                {
                    //resize with free
                    return ReallocResizeCurrFree(header, nextHeader, newSize);
                }
            }
            else
            {
                Header prevHeader = FindPrevOrFirst(header);
                if ((prevHeader.Status == MemoryStatus.Free) && (newSize <= prevHeader.Size + header.Size + nextHeader.Size + 2 * Header.HeaderSize))
                {
                    return ReallocBiggerFreeCurrFree(header, prevHeader, nextHeader, newSize);
                }
                //Not enough free memory to realloc inplace
                return ReallocAllocFree(header, newSize);
            }
        }

        private uint ReallocBiggerFreeCurrFree(Header header, Header prevHeader, Header nextHeader, uint newSize)
        {
            _memory.Copy(prevHeader.DataAddress, header.DataAddress, header.Size);
            if (newSize > prevHeader.Size + header.Size + nextHeader.Size + Header.HeaderSize)
            {
                // concat all 3 blocks
                header.Size += prevHeader.Size + nextHeader.Size + 2 * Header.HeaderSize;
                header.Address = prevHeader.Address;
                header.Write(_memory);
                return header.DataAddress;
            }
            else
            {
                //concat prev free to current
                header.Address = prevHeader.Address;
                header.Size += prevHeader.Size + Header.HeaderSize;
                //resize prec+curr with next
                return ReallocResizeCurrFree(header, nextHeader, newSize);
            }
        }

        private uint ReallocBiggerCurrBusy(Header header, uint newSize)
        {
            Header prevHeader = FindPrevOrFirst(header);
            if ((prevHeader.Status == MemoryStatus.Free) && (newSize <= prevHeader.Size + header.Size + Header.HeaderSize))
            {
                return ReallocBiggerFreeCurrBusy(header, prevHeader, newSize);
            }
            //Not enough free memory to realloc inplace
            return ReallocAllocFree(header, newSize);
        }

        private uint ReallocBiggerFreeCurrBusy(Header header, Header prevHeader, uint newSize)
        {
            _memory.Copy(prevHeader.DataAddress, header.DataAddress, header.Size);
            if (newSize > header.Size + prevHeader.Size)
            {
                //concat both blocks
                header.Size += prevHeader.Size + Header.HeaderSize;
                header.Address = prevHeader.Address;
                header.Write(_memory);
                return header.DataAddress;
            }
            else
            {
                uint newAddr = ReallocResizeFreeCurr(header, prevHeader, newSize);
                _memory.Copy(newAddr, header.DataAddress, header.Size);
                return newAddr;
            }
        }

        private uint ReallocAllocFree(Header header, uint newSize)
        {
            uint oldAddr = header.DataAddress;
            uint newAddr = Alloc(newSize);
            if (newAddr == Null)
            {
                return Null;
            }
            _memory.Copy(newAddr, oldAddr, header.Size > newSize ? newSize : header.Size);
            Free(oldAddr);
            return newAddr;
        }

        private uint ReallocResizeCurrFree(Header header, Header nextHeader, uint newSize)
        {
            uint delta = newSize - header.Size;
            header.Size = newSize;
            header.Write(_memory);

            nextHeader.Address += delta;
            nextHeader.Size -= delta;
            nextHeader.Write(_memory);
            return header.DataAddress;
        }

        private uint ReallocResizeFreeCurr(Header header, Header prevHeader, uint newSize)
        {
            uint delta = newSize - prevHeader.Size;
            header.Size = newSize;
            header.Address -= delta;
            header.Write(_memory);

            prevHeader.Size -= delta;
            prevHeader.Write(_memory);
            return header.DataAddress;
        }
    }
}
