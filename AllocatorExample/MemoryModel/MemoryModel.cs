using System;

namespace MemoryModel
{
    public class MemoryModel
    {
        public MemoryModel(uint size)
        {
            _memory = new byte[size];
        }

        public void FillMemory(byte value)
        {
            for (int i = 0; i < _memory.Length; i++)
            {
                _memory[i] = value;
            }
        }

        public void Write(uint address, byte value)
        {
            _memory[address] = value;
        }

        public byte Read(uint address)
        {
            return _memory[address];
        }

        private byte[] _memory;
    }
}
