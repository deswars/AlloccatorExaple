using AllocatorInterface;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Allocators.AllocationSequence.Actions
{
    class ActionFree
    {
        public ActionFree(int index)
        {
            _index = index;
        }

        public void Action(IAllocator alloc, IList<uint> address)
        {
            Contract.Requires(alloc != null);
            Contract.Requires(address != null);
        
            if (_index < address.Count)
            {
                uint addr = address[_index];
                alloc.Free(addr);
                address.RemoveAt(_index);
            }
        }

        private readonly int _index;
    }
}
