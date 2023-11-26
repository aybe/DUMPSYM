using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolDef : Symbol
{
    public SymbolDef(Stream stream)
    {
        var symbolClass = SymbolUtility.ReadEnum<SymbolDefClass>(stream);
        var symbolType = new SymbolType(stream.Read<ushort>());
        var size = stream.Read<uint>();
        var nameLength = stream.Read<byte>();
        var name = stream.ReadStringAscii(nameLength);
    }
}