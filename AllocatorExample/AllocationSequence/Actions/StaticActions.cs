using AllocatorInterface;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Allocators.AllocationSequence.Actions
{
    public static class StaticActions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API")]
        public static void ActionNull(IAllocator alloc, IList<uint> address)
        {
            return;
        }

        public static void ActionFreeAll(IAllocator alloc, IList<uint> address)
        {
            Contract.Requires(alloc != null);
            Contract.Requires(address != null);

            foreach(var addr in address)
            {
                alloc.Free(addr);
            }
            address.Clear();
        }
    }
}
