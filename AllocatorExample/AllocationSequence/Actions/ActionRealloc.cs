using AllocatorInterface;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Allocators.AllocationSequence.Actions
{
    class ActionRealloc
    {
        public ActionRealloc(int index, uint size)
        {
            _size = size;
            _index = index;
        }

        public void ActionReallocable(IAllocatorReallocable alloc, IList<uint> address)
        {
            Contract.Requires(alloc != null);
            Contract.Requires(address != null);

            if (_index < address.Count)
            {
                uint addr = address[_index];
                uint newAddr = alloc.Realloc(addr, _size);
                if (newAddr != alloc.Null)
                {
                    address.Remove(addr);
                    address.Add(newAddr);

                }
            }
        }

        private readonly int _index;
        private readonly uint _size;
    }
}
