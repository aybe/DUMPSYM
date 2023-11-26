using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolFunctionEnd : Symbol
{
    public SymbolFunctionEnd(Stream stream)
    {
        var line = stream.Read<uint>();
    }
}