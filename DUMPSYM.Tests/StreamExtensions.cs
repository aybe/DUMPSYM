using System.Buffers.Binary;
using System.Text;

namespace DUMPSYM.Tests;

public static class StreamExtensions
{
    public static string ReadStringAscii(this Stream stream, int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        var bytes = new byte[length];

        stream.ReadExactly(bytes);

        var ascii = Encoding.ASCII.GetString(bytes);

        return ascii;
    }

    public static uint ReadUInt16(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];

        stream.ReadExactly(buffer);

        var value = BinaryPrimitives.ReadUInt16LittleEndian(buffer);

        return value;
    }
    
    public static uint ReadUInt32(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];

        stream.ReadExactly(buffer);

        var value = BinaryPrimitives.ReadUInt32LittleEndian(buffer);

        return value;
    }
}