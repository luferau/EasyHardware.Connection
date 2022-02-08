using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyHardware.Connection.Core.Support
{
    public enum TextModeType
    {
        Text,
        Hex,
        Dec,
        Bin
    }

    public static class BytesStringHelper
    {
        public static string ToAsciiString(this byte[] array)
        {
            return Encoding.ASCII.GetString(array);
        }

        public static byte[] ToAsciiBytes(this string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        public static int GetBase(TextModeType mode)
        {
            switch (mode)
            {
                case TextModeType.Text:
                    return 10;
                case TextModeType.Hex:
                    return 16;
                case TextModeType.Dec:
                    return 10;
                case TextModeType.Bin:
                    return 2;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        /// <summary>
        /// Convert string with hex digits to bytes array
        /// F9E39A12 -> {0x12, 0x9a, 0xe3, 0xf9}
        /// </summary>
        /// <param name="text">Hex string</param>
        /// <returns>Bytes array</returns>
        public static byte[] HexStringToBytes(this string text, out bool result)
        {
            // Split the string into two characters substrings and reverse bytes order
            var chunks = text.ChunksSplit(2).Reverse().ToArray();

            var bytes = new List<byte>();
            bytes.AddRange(chunks.ToBytes(GetBase(TextModeType.Hex), out result));

            return bytes.ToArray();
        }

        static IEnumerable<string> ChunksSplit(this string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        public static byte[] ToBytes(this string text, char separator, TextModeType textMode, out bool result)
        {
            var bytes = new List<byte>();
            switch (textMode)
            {
                case TextModeType.Text:
                    bytes.AddRange(text.ToAsciiBytes());
                    result = true;
                    break;
                case TextModeType.Hex:
                case TextModeType.Dec:
                case TextModeType.Bin:
                    var strings = text.Split(separator);
                    bytes.AddRange(strings.ToBytes(GetBase(textMode), out result));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return bytes.ToArray();
        }

        public static byte ToByteFromHexString(this string text)
        {
            return Convert.ToByte(text.Substring(2, 2), 16);
        }

        public static byte[] ToBytes(this string[] strings, int fromBase, out bool result)
        {
            var bytes = new byte[strings.Length];
            try
            {
                for (var i = 0; i < strings.Length; i++)
                {
                    bytes[i] = Convert.ToByte(strings[i], fromBase);
                }
            }
            catch (Exception)
            {
                result = false;
                return Array.Empty<byte>();
            }
            result = true;
            return bytes;
        }

        public static string ToBytesString(this byte[] bytes, char separator, TextModeType textMode)
        {
            string bytesString;
            switch (textMode)
            {
                case TextModeType.Text:
                    bytesString = bytes.ToArray().ToAsciiString();
                    break;
                case TextModeType.Hex:
                case TextModeType.Dec:
                case TextModeType.Bin:
                    var strings = bytes.ToByteStrings(GetBase(textMode));
                    bytesString = string.Join(separator.ToString(), strings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return bytesString;
        }

        public static int GetStringByteLength(int numberBase)
        {
            switch (numberBase)
            {
                case 16:
                    return 2;
                case 10:
                    return 3;
                case 2:
                    return 8;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string[] ToByteStrings(this byte[] bytes, int toBase)
        {
            var strings = new string[bytes.Length];
            for (var i = 0; i < bytes.Length; i++)
            {
                strings[i] = Convert.ToString(bytes[i], toBase).ToUpperInvariant().ToLength(GetStringByteLength(toBase));
            }
            return strings;
        }

        public static string ToLength(this string value, int length, string extraSymbol = "0")
        {
            if (value.Length == length)
                return value;

            if (value.Length > length)
                return value.Substring(value.Length - length, length);

            var builder = new StringBuilder(value);
            builder.Insert(0, extraSymbol, length - value.Length);
            return builder.ToString();
        }

        public static string ToHexStringRepresentation(this byte[] array, int oneLineMaximumBytesLength = 16)
        {
            var hex = new StringBuilder();
            var i = 0;
            foreach (var b in array)
            {
                hex.Append("0x");
                hex.AppendFormat("{0:x2}", b);
                i++;
                if (i == oneLineMaximumBytesLength)
                {
                    i = 0;
                    hex.Append(Environment.NewLine);
                }
                else
                {
                    hex.Append(" ");
                }
            }
            return hex.ToString();
        }
    }
}
