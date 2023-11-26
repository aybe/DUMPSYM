using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolBlockStart : Symbol
{
    public SymbolBlockStart(Stream stream)
    {
        var line = stream.Read<uint>();
    }
}