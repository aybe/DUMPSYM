using Whatever.Extensions;

namespace DUMPSYM;

public record struct SymbolHeader
{
    public SymbolHeader(Stream stream)
    {
        Position = stream.Position;
        Address = stream.Read<uint>();
        Type = stream.Read<byte>();
    }

    public long Position { get; set; }

    public uint Address { get; set; }

    public byte Type { get; set; }

    public override string ToString()
    {
        return $"{Position:x6}: ${Address:x8} {Type:x}";
    }
}