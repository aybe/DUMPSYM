using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolRecordFunctionEnd : SymbolRecord
{
    public SymbolRecordFunctionEnd(Stream stream)
    {
        var line = stream.Read<uint>();
    }
}