using System;
using System.Collections.Generic;
using System.Text;

namespace AllocatorInterface
{
    public class BlockStatus
    {
        public BlockStatus(uint index, uint size, MemoryAnalizerStatus status, IReadOnlyList<BlockStatus> children)
        {
            Index = index;
            Size = size;
            Status = status;
            Children = children;
        }

        public uint Index { get; }
        public uint Size { get; }
        public MemoryAnalizerStatus Status { get; }
        public IReadOnlyList<BlockStatus> Children { get; }
    }
}
