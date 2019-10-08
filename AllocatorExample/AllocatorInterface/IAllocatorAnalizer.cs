using System;
using System.Collections.Generic;
using System.Text;

namespace AllocatorInterface
{
    public enum MemoryAnalizerStatus
    {
        Free = 0, Data = 1, SystemData = 2, Header = 3, SpecialHeader = 4
    }
    public interface IAllocatorAnalizer
    {
        MemoryAnalizerStatus[] AnalizeMemory();
    }
}
