using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolRecordDef : SymbolRecord
{
    public SymbolRecordDef(Stream stream)
    {
        var symbolClass = SymbolUtility.ReadEnum<SymbolStorageClass>(stream);
        var symbolType = new SymbolType(stream.Read<ushort>());
        var size = stream.Read<uint>();
        var nameLength = stream.Read<byte>();
        var name = stream.ReadStringAscii(nameLength);
    }
}