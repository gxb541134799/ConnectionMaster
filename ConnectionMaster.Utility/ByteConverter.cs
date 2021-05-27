using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionMaster.Utility
{
    public class ByteConverter
    {
        private bool isLittleEndian;

        public static ByteConverter BigEndianConverter = new ByteConverter(false);
        public static ByteConverter LittleEndianConverter = new ByteConverter(true);

        private ByteConverter(bool isLittleEndian)
        {
            this.isLittleEndian = isLittleEndian;
        }

        public short ToInt16(byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToInt16(GetFinalBytes(bytes, startIndex, 2), 0);
        }

        public ushort ToUInt16(byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToUInt16(GetFinalBytes(bytes, startIndex, 2), 0);
        }

        public int ToInt32(byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToInt32(GetFinalBytes(bytes, startIndex, 4), 0);
        }

        public uint ToUInt32(byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToUInt32(GetFinalBytes(bytes, startIndex, 4), 0);
        }

        public long ToInt64(byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToInt64(GetFinalBytes(bytes, startIndex, 8), 0);
        }

        public ulong ToUInt64(byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToUInt64(GetFinalBytes(bytes, startIndex, 8), 0);
        }

        public float ToSingle(byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToSingle(GetFinalBytes(bytes, startIndex, 4), 0);
        }

        public double ToDouble(byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToSingle(GetFinalBytes(bytes, startIndex, 8), 0);
        }

        public byte[] GetBytes(short value)
        {
            return GetFinalBytes(BitConverter.GetBytes(value), 0, 2);
        }

        public byte[] GetBytes(ushort value)
        {
            return GetFinalBytes(BitConverter.GetBytes(value), 0, 2);
        }

        public byte[] GetBytes(int value)
        {
            return GetFinalBytes(BitConverter.GetBytes(value), 0, 4);
        }

        public byte[] GetBytes(uint value)
        {
            return GetFinalBytes(BitConverter.GetBytes(value), 0, 4);
        }

        public byte[] GetBytes(long value)
        {
            return GetFinalBytes(BitConverter.GetBytes(value), 0, 8);
        }

        public byte[] GetBytes(ulong value)
        {
            return GetFinalBytes(BitConverter.GetBytes(value), 0, 8);
        }

        public byte[] GetBytes(float value)
        {
            return GetFinalBytes(BitConverter.GetBytes(value), 0, 4);
        }

        public byte[] GetBytes(double value)
        {
            return GetFinalBytes(BitConverter.GetBytes(value), 0, 8);
        }

        private byte[] GetFinalBytes(byte[] origin, int startIndex, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(origin, startIndex, result, 0, result.Length);
            if (isLittleEndian != BitConverter.IsLittleEndian)
            {
                Array.Reverse(result);
            }
            return result;
        }
    }
}
