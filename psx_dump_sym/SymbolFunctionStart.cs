using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolFunctionStart : Symbol
{
    public SymbolFunctionStart(Stream stream)
    {
        var framePointer = stream.Read<ushort>();
        var size = stream.Read<uint>();
        var returnAddressRegister = stream.Read<ushort>();
        var mask = stream.Read<uint>();
        var maskOffset = stream.Read<int>();
        var line = stream.Read<uint>();
        var path = SymbolUtility.ReadStringAscii(stream);
        var name = SymbolUtility.ReadStringAscii(stream);
    }
}