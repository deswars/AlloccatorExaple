﻿using System.Collections.Generic;
using System.Globalization;
using AllocatorInterface;
using MemoryModel;

namespace Allocators.SimpleSLLAllocator
{
    class ReallocAllocatorBuilder : IAllocatorReallocableBuilder
    {
        public IAllocator Build()
        {
            _memory = new Memory(_memorySize);
            return new ReallocAllocator(_memory);
        }

        public IAllocatorReallocable BuildReallocable()
        {
            _memory = new Memory(_memorySize);
            return new ReallocAllocator(_memory);
        }

        public IAllocatorAnalizer BuildAnalizer()
        {
            return new AllocatorAnalizer(_memory);
        }

        public Dictionary<string, string> GetParameterList()
        {
            var result = new Dictionary<string, string>();
            result.Add("MemorySize", defaultMemorySize.ToString(CultureInfo.CurrentCulture));
            return result;
        }

        public void SetParameterList(Dictionary<string, string> list)
        {
            RestoreDefaultValues();
            if ((list != null) && (list.ContainsKey("MemorySize")))
            {
                bool isUint = uint.TryParse(list["MemorySize"], out uint value);
                if (isUint && value >= 100)
                {
                    _memorySize = value;
                }
            }
        }

        uint _memorySize = defaultMemorySize;
        private Memory _memory;
        private const uint defaultMemorySize = 1000;

        private void RestoreDefaultValues()
        {
            _memorySize = defaultMemorySize;
        }
    }
}
