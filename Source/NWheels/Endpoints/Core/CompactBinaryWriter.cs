using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace NWheels.Endpoints.Core
{
    public class CompactBinaryWriter : BinaryWriter
    {
        public CompactBinaryWriter(Stream output)
            : base(output)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompactBinaryWriter(Stream output, Encoding encoding)
            : base(output, encoding)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Write7BitInt(int value)
        {
            Write7BitLong(value);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteCompactDecimal(decimal val)
        {
            string asString = val.ToString(CultureInfo.InvariantCulture);
            // TODO: -=-= Check this on a French localized computer
            string[] split = asString.Split('.'); //System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0]);

            int exponent = 0; 
            if (split.Length > 1)
            {
                exponent = split[1].Length;
            }
            else
            {
                int index = split[0].Length - 1;
                while (index > 0)
                {
                    if (split[0][index] == '0')
                    {
                        exponent--;
                    }
                    else
                    {
                        break;
                    }
                    index--;
                }
            }
            long mantisa = (long)((val * (decimal)Math.Pow(10, exponent))); //InstrumentPrice.ConvertToIntegerPrice(, );
            Write7BitInt(exponent);
            Write7BitLong(mantisa);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        
        public void WriteCompactDecimal(decimal value, int decimalPlaces)
        {
            //Note: if we know the digits in advance - we save the reading/writing of the exponent
            int sign = value > 0 ? 1 : -1;
            Write7BitLong((long)((value * (decimal)Math.Pow(10, decimalPlaces)) + (sign * 0.5M)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public new void Write(string value)
        {
            base.Write(value ?? "");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Write7BitLong(long value)
        {
            bool isNeg = value < 0;
            bool isMinValue = false;
            if (value == long.MinValue)
            {
                isMinValue = true;
                value += 1; //Note: long.MinValue has no equivalent positive value
            }
            value = Math.Abs(value);
            byte mask = 0x3F; //first mask: msb for 'has more' msb-1 for sign
            int shift = 6;
            int loopCount = 0;
            do
            {
                byte lowestByte = (byte)(value & mask);
                value >>= shift;
                if (value != 0)
                    lowestByte |= 0x80;
                if (shift == 6 && isNeg) //shift == 6 only in first byte
                    lowestByte |= 0x40;
                if (loopCount == 9 && isMinValue)
                    lowestByte |= 0x40;
                mask = 0x7F;
                Write(lowestByte);
                shift = 7;
                loopCount++;
            }
            while (value != 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteDateTime(DateTime dateTime)
        {
            Write(dateTime.Ticks);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //ticksResolution: e.g. TimeSpan.TicksPerSecond or TimeSpan.TicksPerhour
        public void WriteCompactDateTime(DateTime dateTime, long ticksResolution)
        {
            Write7BitLong(dateTime.Ticks / ticksResolution);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteTimeSpan(TimeSpan timeSpan)
        {
            Write7BitLong(timeSpan.Ticks);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteBytesArray(byte[] array)
        {
            if (array == null)
            {
                Write7BitInt(0);
            }
            else
            {
                Write7BitInt(array.Length);
                Write(array);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ulong BooleanList2ULong(List<bool> boolsList)
        {
            if (boolsList.Count > 64)
            {
                throw new Exception("SerializationUtils.BooleanList2Long() Error: max list count is 64");
            }
            ulong encodedBools = 0;
            int index = 0;
            foreach (bool b in boolsList)
            {
                ulong bit = (ulong)(b ? 1 : 0);
                bit = bit << index;
                encodedBools = encodedBools | bit;
                index++;
            }

            return encodedBools;
        }

    }
}
