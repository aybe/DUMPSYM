using Whatever.Extensions;

namespace psx_dump_sym;

public static class SymbolUtility
{
    public static Dictionary<SymbolHeader, Symbol> Dump(Stream stream)
    {
        using var scope = stream.SetEndiannessScope(Endianness.LE);

        var header = stream.ReadStringAscii(3);
        if (header != "MND")
        {
            throw new InvalidDataException($"Invalid header: {header}.");
        }

        var version = stream.Read<byte>();
        if (version != 1)
        {
            throw new InvalidDataException($"Invalid version: {version}.");
        }

        var targetUnit = stream.Read<int>();
        if (targetUnit != 0)
        {
            throw new InvalidDataException($"Invalid target unit: {targetUnit}.");
        }

        var symbols = new Dictionary<SymbolHeader, Symbol>();

        while (stream.Position < stream.Length)
        {
            var symbolPosition = stream.Position;

            var symbolHeader = new SymbolHeader(stream);

            Symbol symbol = symbolHeader.Type switch
            {
                0x02 => new SymbolName(stream),
                0x88 => new SymbolSetSldToLineOfFile(stream),
                0x82 => new SymbolIncSldLineNumByByte(stream),
                0x84 => new SymbolIncSldLineNumByWord(stream),
                0x80 => new SymbolIncSldLineNum(stream),
                0x86 => new SymbolSetSldLineNum(stream),
                0x8A => new SymbolEndSldInfo(stream),
                0x8C => new SymbolFunctionStart(stream),
                0x8E => new SymbolFunctionEnd(stream),
                0x94 => new SymbolDef(stream),
                0x96 => new SymbolDef2(stream),
                0x90 => new SymbolBlockStart(stream),
                0x92 => new SymbolBlockEnd(stream),
                _    => throw new NotImplementedException($"0x{symbolHeader.Type:x2} @ {symbolPosition}")
            };

            symbols.Add(symbolHeader, symbol);
        }

        return symbols;
    }

    /// <summary>
    ///     Reads an enum.
    /// </summary>
    public static T ReadEnum<T>(Stream stream) where T : unmanaged, Enum
    {
        var value = stream.Read<T>();

        if (Enum.IsDefined(typeof(T), value))
        {
            return value;
        }

        var integer = Convert.ToInt32(value);

        var message = $"Enum value {integer} (0x{integer:X}) is not defined for {typeof(T)}.";

        throw new InvalidDataException(message);
    }

    /// <summary>
    ///     Reads a length-prefixed (byte) ASCII string.
    /// </summary>
    public static string ReadStringAscii(Stream stream)
    {
        var count = stream.Read<byte>();

        var ascii = stream.ReadStringAscii(count);

        return ascii;
    }
}