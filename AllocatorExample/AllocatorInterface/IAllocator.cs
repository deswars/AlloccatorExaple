using System;
using System.Collections.Generic;
using System.Text;

namespace AllocatorInterface
{
    interface IAllocator
    {
        uint Alloc(uint size);
        bool Free(uint address);
    }
}
