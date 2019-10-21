using AllocatorInterface;
using MemoryModel;
using System.Collections.Generic;
using System.Globalization;

namespace Allocators.ConstantSizeAllocator
{
    public class AllocatorBuilder : IAllocatorBuilder
    {
        public IAllocator Build()
        {
            uint memorySize = _blockCount * _blockSize;
            _memory = new Memory(memorySize);
            return new Allocator(_memory, _blockSize);
        }

        public IAllocatorAnalizer BuildAnalizer()
        {
            return new AllocatorAnalizer(_memory, _blockSize);
        }

        public Dictionary<string, string> GetParameterList()
        {
            var result = new Dictionary<string, string>
            {
                { "BlockSize", defaultBlockSize.ToString(CultureInfo.CurrentCulture) },
                { "BlockCount", defaultBlockCount.ToString(CultureInfo.CurrentCulture) }
            };
            return result;
        }

        public void SetParameterList(Dictionary<string, string> list)
        {
            RestoreDefaultValues();
            if (list != null)
            {
                if ((list.ContainsKey("BlockSize")))
                {
                    bool isUint = uint.TryParse(list["BlockSize"], out uint value);
                    if (isUint && value >= 10)
                    {
                        _blockSize = value;
                    }
                }
                if ((list.ContainsKey("BlockCount")))
                {
                    bool isUint = uint.TryParse(list["BlockCount"], out uint value);
                    if (isUint && value >= 10)
                    {
                        _blockCount = value;
                    }
                }
            }
        }

        uint _blockSize = defaultBlockSize;
        uint _blockCount = defaultBlockCount;
        private Memory _memory;
        private const uint defaultBlockSize = 32;
        private const uint defaultBlockCount = 32;

        private void RestoreDefaultValues()
        {
            _blockSize = defaultBlockSize;
            _blockCount = defaultBlockCount;
        }
    }
}
