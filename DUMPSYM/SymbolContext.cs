using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolContext(Stream stream)
{
    public Stream Stream { get; } = stream;

    public SymbolHeader Header { get; set; }

    public uint Line { get; set; }

    public string ReadStringAscii()
    {
        return Stream.ReadStringAscii(Stream.Read<byte>());
    }

    public T Read<T>(Endianness? endianness = null) where T : unmanaged
    {
        return Stream.Read<T>(endianness);
    }
}