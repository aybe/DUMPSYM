using Whatever.Extensions;

namespace DUMPSYM;

public readonly struct SymbolHeader
{
    public SymbolHeader(Stream stream)
    {
        Position = stream.Position;
        Address = stream.Read<uint>();
        Type = stream.Read<byte>();
    }

    public long Position { get; }

    public uint Address { get; }

    public byte Type { get; }

    public override string ToString()
    {
        return $"{Position:x6}: ${Address:x8} {Type:x}";
    }
}