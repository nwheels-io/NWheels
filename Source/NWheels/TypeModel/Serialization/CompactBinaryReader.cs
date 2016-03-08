using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NWheels.TypeModel.Serialization
{
    public class CompactBinaryReader : BinaryReader
    {

        public CompactBinaryReader(Stream input)
            : base(input)
        {
        }

        public CompactBinaryReader(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of BinaryReader

        public override string ReadString()
        {
            var nullIndicator = ReadByte();

            if (nullIndicator > 0)
            {
                return base.ReadString();
            }

            return null;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int Read7BitInt()
        {
            return (int)Read7BitLong();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        public decimal ReadCompactDecimal()
        {
            int exponent = Read7BitInt();
            long mantisa = Read7BitLong();
            return mantisa / (decimal)Math.Pow(10, exponent);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        public decimal ReadCompactDecimal(int decimalPlaces)
        {
            //Note: if we know the digits in advance - we save the reading/writing of the exponent
            return Read7BitLong() / (decimal)Math.Pow(10, decimalPlaces);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        public long Read7BitLong()
        {
            long result = 0;
            bool isNeg = false;
            int loopCount = 0;
            bool done = false;
            int curShift = 0;
            int nextShiftDiff = 6;
            byte mask = 0x3F; //first mask: msb for 'has more' msb-1 for sign
            bool isMinValue = false; //Note: long.MinValue has no equivalent positive value
            do
            {
                long curByte = ReadByte();
                if (loopCount == 0)
                    isNeg = (curByte & 0x40) > 0;
                if ((curByte & 0x80) == 0)
                    done = true;
                if (loopCount == 9 && (curByte & 0x40) > 0)
                    isMinValue = true;
                curByte &= mask;
                mask = 0x7F;
                result |= (curByte << curShift);
                curShift += nextShiftDiff;
                nextShiftDiff = 7;
                ++loopCount;
            } 
            while (!done);

            if (isMinValue)
                return long.MinValue;
            return isNeg ? -result : result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime ReadDateTime()
        {
            return new DateTime(ReadInt64());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //ticksResolution: e.g. TimeSpan.TicksPerSecond or TimeSpan.TicksPerhour
        public DateTime ReadCompactDateTime(long ticksResolution)
        {
            return new DateTime(Read7BitLong() * ticksResolution);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        public TimeSpan ReadTimeSpan()
        {
            return new TimeSpan(Read7BitLong());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        public Guid ReadGuid()
        {
            return new Guid(ReadBytesArray());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public byte[] ReadBytesArray()
        {
            int count = Read7BitInt();
            return (count > 0 ? ReadBytes(count) : null);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        public static List<bool> ULong2BooleanList(ulong encodedBools, int numOfWantedBooleans)
        {
            if (numOfWantedBooleans > 64)
            {
                throw new Exception("SerializationUtils.ULong2BooleanList() Error: max booleans to extract is 64");
            }
            List<bool> res = new List<bool>(numOfWantedBooleans);
            for (int index = 0; index < numOfWantedBooleans; index++)
            {
                ulong bit = 1;
                bit = bit << index;
                res.Add((encodedBools & bit) > 0);
            }

            return res;
        }

    }
}
