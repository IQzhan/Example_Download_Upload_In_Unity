using System;
using System.IO;

namespace E.Data
{
    public class DataVerification
    {
        #region Convert

        private static byte[] ComputeHash(System.Security.Cryptography.HashAlgorithm calculator, Stream data)
        {
            byte[] result = calculator.ComputeHash(data);
            calculator.Dispose();
            return result;
        }

        private static byte[] ComputeHash(System.Security.Cryptography.HashAlgorithm calculator, byte[] data, int offset = -1, int count = -1)
        {
            byte[] result;
            if (offset != -1 && count != -1)
            {
                result = calculator.ComputeHash(data, offset, count);
            }
            else
            {
                result = calculator.ComputeHash(data);
            }
            calculator.Dispose();
            return result;
        }

        public static string ToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        #endregion

        #region MD5

        public static byte[] ComputeMD5(Stream data)
        {
            return ComputeHash(System.Security.Cryptography.MD5.Create(), data);
        }

        public static byte[] ComputeMD5(byte[] data, int offset = -1, int count = -1)
        {
            return ComputeHash(System.Security.Cryptography.MD5.Create(), data, offset, count);
        }

        public static string ComputeMD5String(Stream data)
        {
            return ToHexString(ComputeMD5(data));
        }

        public static string ComputeMD5String(byte[] data, int offset = -1, int count = -1)
        {
            return ToHexString(ComputeMD5(data, offset, count));
        }

        #endregion

        #region SHA1

        public static byte[] ComputeSHA1(Stream data)
        {
            return ComputeHash(System.Security.Cryptography.SHA1.Create(), data);
        }

        public static byte[] ComputeSHA1(byte[] data, int offset = -1, int count = -1)
        {
            return ComputeHash(System.Security.Cryptography.SHA1.Create(), data, offset, count);
        }

        public static string ComputeSHA1String(Stream data)
        {
            return ToHexString(ComputeSHA1(data));
        }

        public static string ComputeSHA1String(byte[] data, int offset = -1, int count = -1)
        {
            return ToHexString(ComputeSHA1(data, offset, count));
        }

        #endregion

        #region SHA256

        public static byte[] ComputeSHA256(Stream data)
        {
            return ComputeHash(System.Security.Cryptography.SHA256.Create(), data);
        }

        public static byte[] ComputeSHA256(byte[] data, int offset = -1, int count = -1)
        {
            return ComputeHash(System.Security.Cryptography.SHA256.Create(), data, offset, count);
        }

        public static string ComputeSHA256String(Stream data)
        {
            return ToHexString(ComputeSHA256(data));
        }

        public static string ComputeSHA256String(byte[] data, int offset = -1, int count = -1)
        {
            return ToHexString(ComputeSHA256(data, offset, count));
        }

        #endregion

        #region SHA512

        public static byte[] ComputeSHA512(Stream data)
        {
            return ComputeHash(System.Security.Cryptography.SHA512.Create(), data);
        }

        public static byte[] ComputeSHA512(byte[] data, int offset = -1, int count = -1)
        {
            return ComputeHash(System.Security.Cryptography.SHA512.Create(), data, offset, count);
        }

        public static string ComputeSHA512String(Stream data)
        {
            return ToHexString(ComputeSHA512(data));
        }

        public static string ComputeSHA512String(byte[] data, int offset = -1, int count = -1)
        {
            return ToHexString(ComputeSHA512(data, offset, count));
        }

        #endregion

        #region CRC16

        public static byte[] ComputeCRC16(Stream data)
        {
            ushort crc = 0xFFFF;
            long length = data.Length;
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data.Read(bytes, i, 1);
                crc = (ushort)(crc ^ (bytes[i]));
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                }
            }
            return new byte[]
            {
                (byte)((crc & 0xFF00) >> 8),
                (byte)(crc & 0x00FF)
            };
        }

        public static byte[] ComputeCRC16(byte[] data, int offset = -1, int count = -1)
        {
            ushort crc = 0xFFFF;
            offset = offset == -1 ? 0 : offset;
            count = count == -1 ? data.Length : offset + count;
            for (int i = offset; i < count; i++)
            {
                crc = (ushort)(crc ^ (data[i]));
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                }
            }
            return new byte[] 
            { 
                (byte)((crc & 0xFF00) >> 8), 
                (byte)(crc & 0x00FF) 
            };
        }

        public static string ComputeCRC16String(Stream data)
        {
            return ToHexString(ComputeCRC16(data));
        }

        public static string ComputeCRC16String(byte[] data, int offset = -1, int count = -1)
        {
            return ToHexString(ComputeCRC16(data, offset, count));
        }

        #endregion

        #region CRC32

        private static readonly ulong[] CRC32Table = CreateCRC32Table();

        private static ulong[] CreateCRC32Table()
        {
            ulong Crc;
            ulong[] table = new ulong[256];
            int i, j;
            for (i = 0; i < 256; i++)
            {
                Crc = (ulong)i;
                for (j = 8; j > 0; j--)
                {
                    if ((Crc & 1) == 1)
                        Crc = (Crc >> 1) ^ 0xEDB88320;
                    else
                        Crc >>= 1;
                }
                table[i] = Crc;
            }
            return table;
        }

        private static byte[] ToBytes32(ulong data)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)((data >> 24) & 255);
            bytes[1] = (byte)((data >> 16) & 255);
            bytes[2] = (byte)((data >> 8) & 255);
            bytes[3] = (byte)(data & 255);
            return bytes;
        }

        public static byte[] ComputeCRC32(Stream data)
        {
            ulong value = 0xffffffff;
            long length = data.Length;
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data.Read(bytes, i, 1);
                value = (value >> 8) ^ CRC32Table[(value & 0xFF) ^ bytes[i]];
            }
            return ToBytes32(value ^ 0xffffffff);
        }

        public static byte[] ComputeCRC32(byte[] data, int offset = -1, int count = -1)
        {
            ulong value = 0xffffffff;
            offset = offset == -1 ? 0 : offset;
            count = count == -1 ? data.Length : offset + count;
            for (int i = offset; i < count; i++)
            {
                value = (value >> 8) ^ CRC32Table[(value & 0xFF) ^ data[i]];
            }
            return ToBytes32(value ^ 0xffffffff);
        }

        public static string ComputeCRC32String(Stream data)
        {
            return ToHexString(ComputeCRC32(data));
        }

        public static string ComputeCRC32String(byte[] data, int offset = -1, int count = -1)
        {
            return ToHexString(ComputeCRC32(data, offset, count));
        }

        #endregion

    }
}