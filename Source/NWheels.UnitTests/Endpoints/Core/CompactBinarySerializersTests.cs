using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NWheels.TypeModel.Serialization;
using NWheels.Extensions;

namespace NWheels.UnitTests.Endpoints.Core
{
    [TestFixture]
    public class CompactBinarySerializersTests
    {
        [Test]
        public void TestReadWrite7BitLong()
        {
            TestReadWrite7BitLong(1);
            TestReadWrite7BitLong(63);
            TestReadWrite7BitLong(64);
            TestReadWrite7BitLong(65);
            TestReadWrite7BitLong(127);
            TestReadWrite7BitLong(128);
            TestReadWrite7BitLong(129);
            TestReadWrite7BitLong(1290000);
            TestReadWrite7BitLong(-1);
            TestReadWrite7BitLong(-63);
            TestReadWrite7BitLong(-64);
            TestReadWrite7BitLong(-65);
            TestReadWrite7BitLong(-127);
            TestReadWrite7BitLong(-128);
            TestReadWrite7BitLong(-129);
            TestReadWrite7BitLong(long.MaxValue / 3);
            TestReadWrite7BitLong(long.MinValue / 3);
            TestReadWrite7BitLong(long.MaxValue);
            TestReadWrite7BitLong(long.MinValue);
            TestReadWrite7BitLong(long.MinValue + 1);
            TestReadWrite7BitLong(int.MinValue);
        }

        private static void TestReadWrite7BitLong(long num)
        {
            //-----------------------------
            MemoryStream msW = new MemoryStream();
            CompactBinaryWriter bw = new CompactBinaryWriter(msW);
            bw.Write7BitLong(num);
            //-----------------------------
            MemoryStream msR = new MemoryStream(msW.ToArray());
            CompactBinaryReader br = new CompactBinaryReader(msR);
            long parsedNum = br.Read7BitLong();

            Assert.AreEqual(num, parsedNum);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int[] TestReadWrite7BitIntData = new int[] {
            1,
            63,
            64,
            65,
            127,
            128,
            129,
            1290000,
            -1,
            -63,
            -64,
            -65,
            -127,
            -128,
            -129,
            int.MaxValue / 3,
            int.MinValue / 3,
            int.MaxValue,
            int.MinValue
        };
        
        [TestCaseSource("TestReadWrite7BitIntData")]
        public void TestReadWrite7BitInt(int num)
        {
            MemoryStream msW = new MemoryStream();
            CompactBinaryWriter bw = new CompactBinaryWriter(msW);
            bw.Write7BitInt(num);
            //-----------------------------
            MemoryStream msR = new MemoryStream(msW.ToArray());
            CompactBinaryReader br = new CompactBinaryReader(msR);
            int parsedNum = br.Read7BitInt();

            Assert.AreEqual(num, parsedNum);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public decimal[] TestReadWriteCompactDecimalData = new decimal[] {
            0m,
            -100.3m,
            100.3m,
            1003m,
            100300m,
            100000m,
            100000.12345m,
            -100300m,
            -100000m,
            -1003000000000m,
            -1000000000000m,
        };

        [TestCaseSource("TestReadWriteCompactDecimalData")]
        public void TestReadWriteCompactDecimal(decimal num)
        {
            //-- arrange & act

            MemoryStream msW = new MemoryStream();
            CompactBinaryWriter bw = new CompactBinaryWriter(msW);
            bw.WriteCompactDecimal(num);
            //-----------------------------
            MemoryStream msR = new MemoryStream(msW.ToArray());
            CompactBinaryReader br = new CompactBinaryReader(msR);
            decimal parsedNum = br.ReadCompactDecimal();

            //-- assert

            Assert.AreEqual(num, parsedNum);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestReadWriteBytesArray()
        {
            byte[] arr = { 0x3, 0x8, 0x56, 0x0, 0x9, 0x1};
            TestReadWriteBytesArray(arr);
        }

        private static void TestReadWriteBytesArray(byte[] arr)
        {
            MemoryStream msW = new MemoryStream();
            CompactBinaryWriter bw = new CompactBinaryWriter(msW);
            bw.WriteBytesArray(arr);
            //-----------------------------
            MemoryStream msR = new MemoryStream(msW.ToArray());
            CompactBinaryReader br = new CompactBinaryReader(msR);
            byte[] parsedArr = br.ReadBytesArray();

            Assert.AreEqual(arr, parsedArr);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestReadWriteList()
        {
            List<int> intList = new List<int> { 3, 8, 56, 0, 9, 1, 1000, 45345 };
            TestReadWriteList(intList, CompactBinaryReaderWriterExtensions.ReadInt, CompactBinaryReaderWriterExtensions.WriteInt);

            List<decimal> decimalList = new List<decimal> { 3.4353m, 8.23m, 564, 0, 4123, 11, 1, 45345.434m };
            TestReadWriteList(decimalList, CompactBinaryReaderWriterExtensions.ReadCompactDecimal, CompactBinaryReaderWriterExtensions.WriteCompactDecimal);

            List<string> strList = new List<string> { "", "  ", "Hi", "Hello world", null, "FF"  };
            TestReadWriteList(strList, CompactBinaryReaderWriterExtensions.ReadString, CompactBinaryReaderWriterExtensions.WriteString);
        }

        private void TestReadWriteList<T>(
            List<T> list,
            CompactBinaryReaderWriterExtensions.ReadDataDelegate<T> readDlgt,
            CompactBinaryReaderWriterExtensions.WriteDataDelegate<T> writeDlgt )
        {
            MemoryStream msW = new MemoryStream();
            CompactBinaryWriter bw = new CompactBinaryWriter(msW);
            bw.Write(list, writeDlgt, null);
            //-----------------------------
            MemoryStream msR = new MemoryStream(msW.ToArray());
            CompactBinaryReader br = new CompactBinaryReader(msR);
            List<T> parsedList = new List<T>();
            br.Read(parsedList, readDlgt, null);

            Assert.AreEqual(list.Count, parsedList.Count);
            Assert.True(parsedList.TrueForAll( list.Contains));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestReadWriteDictionary()
        {
            Dictionary<int, int> intIntDict = new Dictionary<int,int> { {3, 8}, {56, 0}, {9, 1}, {900, 45345} };
            TestReadWriteDictionary(
                intIntDict,
                CompactBinaryReaderWriterExtensions.ReadInt,
                CompactBinaryReaderWriterExtensions.WriteInt,
                CompactBinaryReaderWriterExtensions.ReadInt,
                CompactBinaryReaderWriterExtensions.WriteInt);

            Dictionary<int, string> intStrDict = new Dictionary<int, string> { { 3, "8" }, { 56, "0" }, { 9, "1" }, { 900, "45345" } };
            TestReadWriteDictionary(
                intStrDict,
                CompactBinaryReaderWriterExtensions.ReadInt,
                CompactBinaryReaderWriterExtensions.WriteInt,
                CompactBinaryReaderWriterExtensions.ReadString,
                CompactBinaryReaderWriterExtensions.WriteString);
        }

        private void TestReadWriteDictionary<TK, TV>(
            Dictionary<TK,TV> dict,
            CompactBinaryReaderWriterExtensions.ReadDataDelegate<TK> readKeyDlgt,
            CompactBinaryReaderWriterExtensions.WriteDataDelegate<TK> writeKeyDlgt,
            CompactBinaryReaderWriterExtensions.ReadDataDelegate<TV> readValDlgt,
            CompactBinaryReaderWriterExtensions.WriteDataDelegate<TV> writeValDlgt)
        {
            MemoryStream msW = new MemoryStream();
            CompactBinaryWriter bw = new CompactBinaryWriter(msW);
            bw.Write(dict, writeKeyDlgt, null, writeValDlgt, null);
            //-----------------------------
            MemoryStream msR = new MemoryStream(msW.ToArray());
            CompactBinaryReader br = new CompactBinaryReader(msR);
            Dictionary<TK,TV> parsedDict = new Dictionary<TK, TV>();
            br.Read(parsedDict, readKeyDlgt, null, readValDlgt, null);

            Assert.AreEqual(dict.Count, parsedDict.Count);
            Assert.True(parsedDict.Keys.ToList().TrueForAll(dict.ContainsKey));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestBooleanList()
        {
            List<bool> emptyList = new List<bool>();
            TestBooleanList(emptyList);

            List<bool> list1 = new List<bool> { true, false, false, true };
            TestBooleanList(list1);

            List<bool> list2 = new List<bool> { false };
            TestBooleanList(list2);

            List<bool> list3 = new List<bool> {
                true, false, false, true, true, false, true, false,
                true, false, false, true, true, false, false, true,
                true, false, false, true, true, true, true, true,
                true, false, false, true, false, false, true, true
            };
            TestBooleanList(list3);
        }

        public void TestBooleanList(List<bool> list)
        {
            ulong longRes = CompactBinaryWriter.BooleanList2ULong(list);
            //-----------------------------
            List<bool> parsedList = CompactBinaryReader.ULong2BooleanList(longRes, list.Count);

            Assert.AreEqual(list.Count, parsedList.Count);
            for ( int i = 0 ; i < list.Count ; i++ )
            {
                Assert.AreEqual(list[i], parsedList[i]);
            }
        }
    }
}
