using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolSetSldToLineOfFile(Stream stream) : Symbol
{
    public uint Line { get; } = stream.Read<uint>();

    public string Path { get; } = stream.ReadStringAscii(stream.Read<byte>());

    public override string ToString()
    {
        return $"{nameof(Line)}: {Line}, {nameof(Path)}: {Path}";
    }
}