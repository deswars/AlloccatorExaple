using AllocatorInterface;
using MemoryModel;

namespace Allocators.SLLAllocator
{
    public struct Header
    {
        public const uint AddressSize = sizeof(uint);
        public const uint Size = AddressSize * 2;
        public const uint StatusMask = AddressSize - 1;
        public const uint SizeMask = ~StatusMask;

        public uint NextAddress;
        public uint Mixed;

        public uint GetSize()
        {
            return Mixed & SizeMask;
        }

        public MemoryStatus GetStatus()
        {
            return (MemoryStatus)(Mixed & StatusMask);
        }

        public void SetMixed(uint size, MemoryStatus status)
        {
            Mixed = (size & SizeMask) | ((uint)status & StatusMask);
        }

        public void SetStatus(MemoryStatus status)
        {
            Mixed = (Mixed & SizeMask) | ((uint)status & StatusMask);
        }

        public void SetSize(uint size)
        {
            Mixed = (Mixed & StatusMask) | (size & SizeMask);
        }

        public static uint GenerateMixed(uint size, MemoryStatus status)
        {
            return (size & SizeMask) | ((uint)status & StatusMask);
        }
    }
}
