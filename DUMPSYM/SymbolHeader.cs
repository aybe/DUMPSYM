using Whatever.Extensions;

namespace DUMPSYM;

public readonly struct SymbolHeader(Stream stream)
{
    public long Position { get; } = stream.Position;

    public uint Address { get; } = stream.Read<uint>();

    public byte Type { get; } = stream.Read<byte>();

    public override string ToString()
    {
        return $"{Position:x6}: ${Address:x8} {Type:x}";
    }
}