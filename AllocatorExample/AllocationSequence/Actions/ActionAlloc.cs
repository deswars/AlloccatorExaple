using AllocatorInterface;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Allocators.AllocationSequence.Actions
{
    public class ActionAlloc
    {
        public ActionAlloc(uint size)
        {
            _size = size;
        }

        public void Action(IAllocator alloc, IList<uint> address)
        {
            Contract.Requires(alloc != null);
            Contract.Requires(address != null);

            uint addr = alloc.Alloc(_size);
            if (addr != alloc.Null)
            {
                address.Add(addr);
            }
        }

        private readonly uint _size;
    }
}
