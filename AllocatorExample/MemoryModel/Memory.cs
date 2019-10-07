using System;

namespace MemoryModel
{
    public class Memory
    {
        public Memory(uint size)
        {
            _memory = new byte[size];
        }

        public void FillMemory(byte value)
        {
            for (uint i = 0; i < _memory.Length; i++)
            {
                _memory[i] = value;
            }
        }

        public void Write(uint address, byte value)
        {
            if (address < _memory.Length)
            {
                _memory[address] = value;
            }
        }

        public byte Read(uint address)
        {
            if (address >= _memory.Length)
            {
                return 0;
            }
            return _memory[address];
        }

        public void WriteBytes(uint address, byte[] value)
        {
            if (address + value.Length > _memory.Length)
            {
                return;
            }
            for (uint i = 0; i < value.Length; i++)
            {
                _memory[address + i] = value[i];
            }
        }

        public byte[] ReadBytes(uint address, uint size)
        {
            if (address + size > _memory.Length)
            {
                return new byte[0];
            }
            var result = new byte[size];
            for ( uint i = 0; i < size; i++)
            {
                result[i] = _memory[address + i];
            }
            return result;
        }

        public uint BytesToWord(byte[] value)
        {
            if (value.Length != wordSize)
            {
                return 0;
            }
            uint result = 0;
            for(uint i = 0; i < wordSize; i++)
            {
                result = nextByteValue * result + value[i];
            }
            return result;
        }
        
        public byte[] WordToBytes(uint value)
        {
            var result = new byte[wordSize];
            uint current = value;
            for (uint i = 0; i < wordSize; i++)
            {
                result[wordSize - i - 1] = (byte)(current % nextByteValue);
                current = current / nextByteValue;
            }
            return result;
        }

        public void WriteWord(uint address, uint value)
        {
            var bytes = WordToBytes(value);
            WriteBytes(address, bytes);
        }

        public uint ReadWord(uint address)
        {
            var bytes = ReadBytes(address, wordSize);
            return BytesToWord(bytes);
        }

        public uint Size()
        {
            return (uint)_memory.Length;
        }

        private const uint wordSize = sizeof(uint);
        private const uint nextByteValue = 256;

        private byte[] _memory;
    }
}
