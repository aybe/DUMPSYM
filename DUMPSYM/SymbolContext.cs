using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolContext(Stream stream)
{
    private Stream Stream { get; } = stream;

    public SymbolHeader Header { get; set; }

    public uint Line { get; set; }

    public T Read<T>(Endianness? endianness = null) where T : unmanaged
    {
        return Stream.Read<T>(endianness);
    }

    public string ReadStringAscii()
    {
        return Stream.ReadStringAscii(Stream.Read<byte>());
    }
}