using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolHeader(Stream stream)
{
    public long Position { get; } = stream.Position;

    public uint Address { get; } = stream.Read<uint>();

    public byte Type { get; } = stream.Read<byte>();

    public override string ToString()
    {
        return $"{nameof(Position)}: {Position}, {nameof(Address)}: 0x{Address:X8}, {nameof(Type)}: 0x{Type:X2}";
    }

    public void WriteHeaderPositionAddress(TextWriter writer)
    {
        writer.Write($"{Position:x6}: ");
        writer.Write($"${Address:x8} ");
    }

    public void WriteHeaderPositionAddressType(TextWriter writer)
    {
        WriteHeaderPositionAddress(writer);

        writer.Write($"{Type:x} ");
    }
}