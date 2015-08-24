using System;
using System.Collections.Generic;
using NWheels.TypeModel.Serialization;

namespace NWheels.Extensions
{
    public static class CompactBinaryReaderWriterExtensions
    {
        public static string ReadString(this CompactBinaryReader br, object context)
        {
            return br.ReadString();
        }

        public static void WriteString(this CompactBinaryWriter bw, string val, object context)
        {
            bw.Write(val);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static int ReadInt(this CompactBinaryReader br, object context)
        {
            return br.Read7BitInt();
        }

        public static void WriteInt(this CompactBinaryWriter bw, int val, object context)
        {
            bw.Write7BitInt(val);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        // Uses reflection - relatively slow!!!
        public static T ReadEnum<T>(this CompactBinaryReader br, object context) where T : IConvertible
        {
            long val = br.Read7BitLong();
            return (T)Enum.ToObject(typeof(T), val);
        }

        public static void WriteEnum<T>(this CompactBinaryWriter bw, T val, object context) where T : IConvertible
        {
            bw.Write7BitLong((long)Convert.ChangeType(val, typeof(long)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static long ReadLong(this CompactBinaryReader br, object context)
        {
            return br.Read7BitLong();
        }

        public static void WriteLong(this CompactBinaryWriter bw, long val, object context)
        {
            bw.Write7BitLong(val);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static byte[] ReadBytesArray(this CompactBinaryReader br, object context)
        {
            return br.ReadBytesArray();
        }

        public static void WriteBytesArray(this CompactBinaryWriter bw, byte[] val, object context)
        {
            bw.WriteBytesArray(val);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static decimal ReadCompactDecimal(this CompactBinaryReader br, object context)
        {
            return br.ReadCompactDecimal();
        }

        public static void WriteCompactDecimal(CompactBinaryWriter bw, decimal val, object context)
        {
            bw.WriteCompactDecimal(val);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ReadBool(this CompactBinaryReader br, object context)
        {
            return br.ReadBoolean();
        }

        public static void WriteBool(this CompactBinaryWriter bw, bool val, object context)
        {
            bw.Write(val);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Write<T>(this CompactBinaryWriter bw, List<T> list, WriteDataDelegate<T> itemWriter, object context)
        {
            if (list != null)
            {
                bw.Write7BitInt(list.Count);
                foreach (T item in list)
                {
                    itemWriter(bw, item, context);
                }
            }
            else
            {
                bw.Write7BitInt(0);
            }
        }

        public static void Read<T>(this CompactBinaryReader br, List<T> list, ReadDataDelegate<T> itemReader, object context)
        {
            //Clear the list before work starts. In case the list is null it will throw exception
            list.Clear();
            int count = br.Read7BitInt();
            for (int i = 0; i < count; i++)
            {
                T item = itemReader(br, context);
                list.Add(item);
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Write<TK, TV>(
            this CompactBinaryWriter bw,
            IDictionary<TK, TV> map,
            WriteDataDelegate<TK> keyWriter, object keyContext,
            WriteDataDelegate<TV> valWriter, object valContext)
        {
            bw.Write7BitInt(map.Count);
            foreach (KeyValuePair<TK, TV> keyValue in map)
            {
                keyWriter(bw, keyValue.Key, keyContext);
                valWriter(bw, keyValue.Value, valContext);
            }
        }

        public static void Read<TK, TV>(
            this CompactBinaryReader br,
            IDictionary<TK, TV> map,
            ReadDataDelegate<TK> keyReader, object keyContext,
            ReadDataDelegate<TV> valReader, object valContext)
        {
            int count = br.Read7BitInt();

            for ( int i = 0 ; i < count ; i++ )
            {
                TK key = keyReader(br, keyContext);
                TV val = valReader(br, valContext);
                map.Add(key, val);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate T ReadDataDelegate<T>(CompactBinaryReader br, object context);
        public delegate void WriteDataDelegate<T>(CompactBinaryWriter bw, T instance, object context);

    }
}
