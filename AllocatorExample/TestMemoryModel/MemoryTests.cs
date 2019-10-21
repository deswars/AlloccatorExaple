using Xunit;

namespace MemoryModel.Tests
{
    public class MemoryTests
    {
        const uint size = 10;
        readonly Memory memory;

        public MemoryTests()
        {
            memory = new Memory(size);
        }

        [Fact]
        public void FillMemoryTest()
        {
            byte filler = 0xAC;

            memory.FillMemory(filler);
            for (uint i = 0; i < size; i++)
            {
                Assert.Equal(filler, memory.Read(i));
            }
        }

        [Fact]
        public void ReadWriteTest()
        {
            byte filler = 0xAC;
            uint address = 2;

            memory.Write(address, filler);
            Assert.Equal(filler, memory.Read(address));

            //write after last without exception
            memory.Write(size, filler);

            Assert.Equal(0, memory.Read(size));
        }

        [Fact]
        public void ReadBytesTest()
        {
            byte filler = 5;
            uint address = 2;

            memory.Write(address, filler);
            var bytes1 = memory.ReadBytes(2, 0);
            Assert.Empty(bytes1);

            var bytes2 = memory.ReadBytes(2, 1);
            Assert.Single(bytes2);
            Assert.Equal(5, bytes2[0]);

            var bytes3 = memory.ReadBytes(1, 3);
            Assert.Equal(3, bytes3.Length);
            Assert.Equal(0, bytes3[0]);
            Assert.Equal(5, bytes3[1]);
            Assert.Equal(0, bytes3[2]);

            //out of memory 
            var bytes4 = memory.ReadBytes(2, 10);
            Assert.Empty(bytes4);
        }

        [Fact]
        public void WriteBytesTest()
        {
            byte[] bytes1 = { 1, 2 };
            uint address1 = 2;
            memory.WriteBytes(address1, bytes1);
            Assert.Equal(1, memory.Read(2));
            Assert.Equal(2, memory.Read(3));

            //out of memory write does nothing
            byte[] bytes2 = { 2, 3 };
            uint address2 = 9;
            memory.WriteBytes(address2, bytes2);
            Assert.Equal(0, memory.Read(9));
        }

        [Fact]
        public void BytesToWordTest()
        {
            byte[] bytes1 = { 0, 0, 0, 5 };
            uint word1 = 5;
            Assert.Equal(word1, Memory.BytesToWord(bytes1));

            byte[] bytes2 = { 0, 0, 1, 1 };
            uint word2 = 1 * 256 + 1;
            Assert.Equal(word2, Memory.BytesToWord(bytes2));

            byte[] bytes3 = { 255, 255, 255, 255 };
            uint word3 = uint.MaxValue;
            Assert.Equal(word3, Memory.BytesToWord(bytes3));

            // too small array
            byte[] bytes4 = { 0, 0, 0 };
            Assert.Equal(0u, Memory.BytesToWord(bytes4));

            // too big array
            byte[] bytes5 = { 0, 0, 0, 0, 0 };
            Assert.Equal(0u, Memory.BytesToWord(bytes5));
        }

        [Fact]
        public void WordToBytesTest()
        {
            uint word1 = 5;
            uint word2 = 257;
            uint word3 = uint.MaxValue;

            var bytes1 = Memory.WordToBytes(word1);
            Assert.Equal(4, bytes1.Length);
            Assert.Equal(0, bytes1[0]);
            Assert.Equal(0, bytes1[1]);
            Assert.Equal(0, bytes1[2]);
            Assert.Equal(5, bytes1[3]);

            var bytes2 = Memory.WordToBytes(word2);
            Assert.Equal(4, bytes2.Length);
            Assert.Equal(0, bytes2[0]);
            Assert.Equal(0, bytes2[1]);
            Assert.Equal(1, bytes2[2]);
            Assert.Equal(1, bytes2[3]);

            var bytes3 = Memory.WordToBytes(word3);
            Assert.Equal(4, bytes3.Length);
            Assert.Equal(255, bytes3[0]);
            Assert.Equal(255, bytes3[1]);
            Assert.Equal(255, bytes3[2]);
            Assert.Equal(255, bytes3[3]);
        }

        [Fact]
        public void ReadWordTest()
        {
            uint addres = 2;
            uint word = 257;
            var bytes = Memory.WordToBytes(word);
            memory.WriteBytes(addres, bytes);
            Assert.Equal(word, memory.ReadWord(addres));

            // out of memory read returns 0
            uint address2 = 9;
            Assert.Equal(0, memory.Read(address2));
        }

        [Fact]
        public void WriteWordTest()
        {
            uint address = 2;
            uint word = 257;
            memory.WriteWord(address, word);
            Assert.Equal(word, memory.ReadWord(address));

            //out of memory write does nothing
            uint address2 = 9;
            uint word2 = uint.MaxValue;
            memory.WriteWord(address2, word2);
            Assert.Equal(0, memory.Read(address));
        }

        [Fact]
        public void CopyTest()
        {
            uint addr1 = 1;
            uint addr2 = 2;
            uint addr3 = 8;
            uint size1 = 2;
            uint size2 = 3;

            memory.Write(addr3, 10);
            memory.Write(addr3 + 1, 11);

            //out of boundary dest
            memory.Copy(addr3, addr2, size2);
            Assert.Equal(10, memory.Read(addr3));
            Assert.Equal(11, memory.Read(addr3 + 1));

            //out of boundary source
            memory.Copy(addr2, addr3, size2);
            Assert.Equal(0, memory.Read(addr2));
            Assert.Equal(0, memory.Read(addr2 + 1));

            //normal copy
            memory.Copy(addr2, addr3, size1);
            Assert.Equal(10, memory.Read(addr2));
            Assert.Equal(11, memory.Read(addr2 + 1));

            //overlaping copy
            memory.Copy(addr1, addr2, size1);
            Assert.Equal(10, memory.Read(addr1));
            Assert.Equal(11, memory.Read(addr1 + 1));
            Assert.Equal(11, memory.Read(addr1 + 2));
        }
    }
}
