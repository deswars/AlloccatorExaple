using MemoryModel;
using System.Collections.Generic;

namespace AllocatorInterface
{
    public interface IAllocatorBuilder
    {
        IAllocator Build();
        IAllocatorAnalizer BuildAnalizer();
        Dictionary<string, string> GetParameterList();
        void SetParameterList(Dictionary<string, string> list);
    }
}
