using Whatever.Extensions;

namespace DUMPSYM;

[Serializable]
public record struct SymbolHeader
{
    public SymbolHeader()
    {
    }

    public SymbolHeader(Stream stream)
    {
        Position = stream.Position;
        Address = stream.Read<uint>();
        Type = stream.Read<byte>();
    }

    public long Position { get; set; }

    public uint Address { get; set; }

    public byte Type { get; set; }

    public static IComparer<SymbolHeader> AddressComparer { get; } =
        Comparer<SymbolHeader>.Create((x, y) => x.Address.CompareTo(y.Address));

    public static IComparer<SymbolHeader> PositionComparer { get; } =
        Comparer<SymbolHeader>.Create((x, y) => x.Position.CompareTo(y.Position));

    public override string ToString()
    {
        return $"{Position:x6}: ${Address:x8} {Type:x}";
    }
}