using System.Buffers.Binary;
using System.Text;

namespace DUMPSYM.Extensions;

public static class StreamExtensions
{
    extension(Stream stream)
    {
        public string ReadStringAscii(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);

            Span<byte> buffer = stackalloc byte[length];

            stream.ReadExactly(buffer);

            var value = Encoding.ASCII.GetString(buffer);

            return value;
        }

        public uint ReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];

            stream.ReadExactly(buffer);

            var value = BinaryPrimitives.ReadUInt16LittleEndian(buffer);

            return value;
        }

        public uint ReadUInt32()
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];

            stream.ReadExactly(buffer);

            var value = BinaryPrimitives.ReadUInt32LittleEndian(buffer);

            return value;
        }
    }
}