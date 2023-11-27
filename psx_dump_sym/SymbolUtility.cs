using System.Runtime.CompilerServices;
using Whatever.Extensions;

namespace psx_dump_sym;

public static class SymbolUtility
{
    public static SymbolFile Dump(Stream stream)
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
        
        var symbols = new LinkedList<Symbol>();

        while (stream.Position < stream.Length)
        {
            var symbolPosition = stream.Position;

            var symbolHeader = new SymbolHeader(stream);

            SymbolRecord symbolRecord = symbolHeader.Type switch
            {
                0x01 => new SymbolRecordName(stream),
                0x02 => new SymbolRecordName(stream),
                0x06 => new SymbolRecordName(stream),
                0x88 => new SymbolRecordSetSldToLineOfFile(stream),
                0x82 => new SymbolRecordIncSldLineNumByByte(stream),
                0x84 => new SymbolRecordIncSldLineNumByWord(stream),
                0x80 => new SymbolRecordIncSldLineNum(),
                0x86 => new SymbolRecordSetSldLineNum(stream),
                0x8A => new SymbolRecordEndSldInfo(),
                0x8C => new SymbolRecordFunctionStart(stream),
                0x8E => new SymbolRecordFunctionEnd(stream),
                0x94 => new SymbolRecordDef(stream),
                0x96 => new SymbolRecordDef2(stream),
                0x98 => new SymbolRecordOverlay(stream),
                0x90 => new SymbolRecordBlockStart(stream),
                0x92 => new SymbolRecordBlockEnd(stream),
                0x9A => new SymbolRecordSetOverlay(),
                0x9C => new SymbolRecordFunction2Start(stream),
                _    => throw new NotImplementedException($"0x{symbolHeader.Type:x2} @ {symbolPosition}")
            };

            symbols.AddLast(new Symbol(symbolHeader, symbolRecord));
        }

        return new SymbolFile(header, version, targetUnit, symbols);
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

        var message = $"Enum value {integer} (0x{integer:X}) is not defined for {typeof(T).Name} at position {stream.Position - Unsafe.SizeOf<T>()}.";

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