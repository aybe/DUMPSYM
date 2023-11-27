using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordBlockStart(Stream stream) : SymbolRecord
{
    public uint Line { get; } = stream.Read<uint>();
}