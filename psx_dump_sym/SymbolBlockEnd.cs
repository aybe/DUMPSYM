using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolBlockEnd : Symbol
{
    public SymbolBlockEnd(Stream stream)
    {
        var line = stream.Read<uint>();
    }
}