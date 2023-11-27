using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordSetSldToLineOfFile(Stream stream) : SymbolRecord
{
    public uint Line { get; } = stream.Read<uint>();

    public string Path { get; } = stream.ReadStringAscii(stream.Read<byte>());
}