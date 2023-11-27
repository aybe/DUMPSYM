using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordOverlay(Stream stream) : SymbolRecord
{
    public uint Length { get; } = stream.Read<uint>();

    public uint Id { get; } = stream.Read<uint>();
}