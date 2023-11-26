using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolDef2 : Symbol
{
    public SymbolDef2(Stream stream)
    {
        var position = stream.Position;
        var symbolClass = SymbolUtility.ReadEnum<SymbolDefClass>(stream);

        var symbolType = new SymbolType(stream.Read<ushort>());

        var size = stream.Read<uint>();
        var dimsLen = stream.Read<ushort>();
        for (var i = 0; i < dimsLen; i++)
        {
            stream.Read<uint>(); // TODO
        }

        var tag = SymbolUtility.ReadStringAscii(stream);
        var name = SymbolUtility.ReadStringAscii(stream);
    }
}