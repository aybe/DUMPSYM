using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolRecordBlockStart : SymbolRecord
{
    public SymbolRecordBlockStart(Stream stream)
    {
        var line = stream.Read<uint>();
    }
}