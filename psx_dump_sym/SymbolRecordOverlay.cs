using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordOverlay : SymbolRecord
{
    public SymbolRecordOverlay(Stream stream)
    {
        var length = stream.Read<uint>();
        var id = stream.Read<uint>();
    }
}