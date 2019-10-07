using System;
using System.Collections.Generic;
using System.Text;

namespace AllocatorInterface
{
    interface IAllocatorReallocable : IAllocator
    {
        public uint Relloc(uint address, uint newSize);
    }
}
