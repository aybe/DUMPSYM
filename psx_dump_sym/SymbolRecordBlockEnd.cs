using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolRecordBlockEnd : SymbolRecord
{
    public SymbolRecordBlockEnd(Stream stream)
    {
        var line = stream.Read<uint>();
    }
}