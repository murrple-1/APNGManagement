using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APNGLib
{
	public static class PNGUtils
	{
		public static byte ParseByte(byte[] buffer)
		{
			int offset = 0;
			return ParseByte(buffer, ref offset);
		}

		public static byte ParseByte(byte[] buffer, ref int offset)
		{
			byte value = buffer[offset];
			offset += sizeof(byte);
			return value;
		}

		public static ushort ParseUshort(byte[] buffer)
		{
			int offset = 0;
			return ParseUshort(buffer, ref offset);
		}

		public static ushort ParseUshort(byte[] buffer, ref int offset)
		{
			ushort value = 0;
			if (buffer.Length - offset < sizeof(ushort))
			{
				throw new ArgumentException(
					String.Format("buffer is not long enough to extract {0} bytes at offset {1}",
					sizeof(ushort), offset));
			}
			for (int i = offset + sizeof(ushort) - 1, j = 0; i >= offset; i--, j++)
			{
				value |= (ushort)(buffer[i] << (8 * j));
			}
			offset += sizeof(ushort);
			return value;
		}

		public static uint ParseUint(byte[] buffer)
		{
			int offset = 0;
			return ParseUint(buffer, ref offset);
		}

		public static uint ParseUint(byte[] buffer, ref int offset)
		{
			uint value = 0;
			if (buffer.Length - offset < sizeof(uint))
			{
				throw new ArgumentException(
					String.Format("buffer is not long enough to extract {0} bytes at offset {1}",
					sizeof(uint), offset));
			}
			for (int i = offset + sizeof(uint) - 1, j = 0; i >= offset; i--, j++)
			{
				value |= (uint)(buffer[i] << (8 * j));
			}
			offset += sizeof(uint);
			return value;
		}

		public static string ParseString(byte[] buffer, int length)
		{
			int offset = 0;
			return ParseString(buffer, ref offset, length);
		}

		public static string ParseString(byte[] buffer, ref int offset, int length)
		{
			StringBuilder sb = new StringBuilder();
			if (buffer.Length - offset < length)
			{
				throw new ArgumentException(
					String.Format("buffer is not long enough to extract {0} bytes at offset {1}",
					length, offset));
			}
			for (int i = offset; i < (offset + length); i++)
			{
				sb.Append((char)buffer[i]);
			}
			offset += length;
			return sb.ToString();
		}


		public static string ParseString(byte[] buffer)
		{
			int offset = 0;
			return ParseString(buffer, ref offset);
		}

		public static string ParseString(byte[] buffer, ref int offset)
		{
			if (buffer.Length <= offset)
			{
				throw new ArgumentException(
					String.Format("buffer is not long enough to extract string at offset {0}", offset));
			}
			StringBuilder sb = new StringBuilder();
			char curr = (char)buffer[offset];
			do
			{
				sb.Append(curr);
				curr = (char)buffer[++offset];
			}
			while (curr != '\0' && offset < (buffer.Length - 1));
			return sb.ToString();
		}

		public static byte[] ParseByteArray(byte[] buffer, int length)
		{
			int offset = 0;
			return ParseByteArray(buffer, ref offset, length);
		}

		public static byte[] ParseByteArray(byte[] buffer, ref int offset, int length)
		{
			byte[] value = new byte[length];
			if (buffer.Length - offset < length)
			{
				throw new ArgumentException(
					String.Format("buffer is not long enough to extract {0} bytes at offset {1}",
					length, offset));
			}
			Array.Copy(buffer, offset, value, 0, length);
			return value;
		}

		public static byte[] Combine(params byte[][] arrays)
		{
			byte[] rv = new byte[arrays.Sum(a => a.Length)];
			int offset = 0;
			foreach (byte[] array in arrays)
			{
				System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
				offset += array.Length;
			}
			return rv;
		}

		public static byte[] GetBytes(byte b)
		{
			return new byte[] { b };
		}

		public static byte[] GetBytes(ushort s)
		{
			byte[] value = BitConverter.GetBytes(s);
			Array.Reverse(value);
			return value;
		}

		public static byte[] GetBytes(uint i)
		{
			byte[] value = BitConverter.GetBytes(i);
			Array.Reverse(value);
			return value;
		}
	}
}
