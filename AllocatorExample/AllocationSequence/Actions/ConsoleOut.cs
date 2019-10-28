using AllocatorInterface;
using System;
using System.Collections.Generic;

namespace Allocators.AllocationSequence.Actions
{
    public class ConsoleOut
    {
        public ConsoleOut(string data)
        {
            _data = data;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API")]
        public void ConsoleAction(IAllocator alloc, IList<uint> address)
        {
            Console.WriteLine(_data);
            return;
        }

        private readonly string _data;
    }
}
