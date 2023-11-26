using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolOverlay : Symbol
{
    public SymbolOverlay(Stream stream)
    {
        var length = stream.Read<uint>();
        var id = stream.Read<uint>();
    }
}