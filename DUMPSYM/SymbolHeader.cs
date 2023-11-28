using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolHeader(Stream stream)
{
    public long Position { get; } = stream.Position;

    public uint Address { get; } = stream.Read<uint>();

    public byte Type { get; } = stream.Read<byte>();

    public override string ToString()
    {
        return $"{Position:x6}: ${Address:x8} {Type:x}";
    }

    public void WriteHeaderPositionAddress(TextWriter writer)
    {
        writer.Write($"{Position:x6}: ");
        writer.Write($"${Address:x8} ");
    }

    public void WriteHeaderPositionAddressType(TextWriter writer) // TODO let users add space instead?
    {
        WriteHeaderPositionAddress(writer);

        writer.Write($"{Type:x} ");
    }
}