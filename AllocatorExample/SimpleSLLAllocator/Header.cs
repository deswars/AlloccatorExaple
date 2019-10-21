using AllocatorInterface;
using MemoryModel;
using System.Diagnostics.Contracts;

namespace Allocators.SimpleSLLAllocator
{
    public struct Header : System.IEquatable<Header>
    {
        public const uint AddressSize = sizeof(uint);
        public const uint HeaderSize = AddressSize;
        public const uint StatusMask = AddressSize - 1;
        public const uint SizeMask = ~StatusMask;

#pragma warning disable CA1051 // Do not declare visible instance fields
        public uint Address;
        public uint Size;
        public MemoryStatus Status;
#pragma warning restore CA1051 // Do not declare visible instance fields
        public uint NextAddress
        {
            get
            {
                return Address + Size + HeaderSize;
            }
        }
        public uint DataAddress
        {
            get
            {
                return Address + HeaderSize;
            }
        }

        public static Header Read(Memory memory, uint address)
        {
            Contract.Requires(memory != null);

            uint packed = memory.ReadWord(address);
            Header result;
            result.Address = address;
            result.Size = packed & SizeMask;
            result.Status = (MemoryStatus)(packed & StatusMask);
            return result;
        }

        public static Header ReadDataHeader(Memory memory, uint dataAddress)
        {
            return Read(memory, GetHeaderAddress(dataAddress));
        }

        public static uint GetHeaderAddress(uint dataAddress)
        {
            return dataAddress - HeaderSize;
        }

        public void Write(Memory memory)
        {
            Contract.Requires(memory != null);

            uint packed = Size | (uint)Status;
            memory.WriteWord(Address, packed);
        }

        public bool Equals(Header other)
        {
            return (Address == other.Address) && (Size == other.Size) && (Status == other.Status);
        }

        public override bool Equals(object obj)
        {
            return obj is Header && Equals((Header)obj);
        }

        public static bool operator ==(Header left, Header right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Header left, Header right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }
    }
}
