using System.Runtime.CompilerServices;
using Whatever.Extensions;

namespace DUMPSYM;

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

        var targetUnit = stream.Read<byte>();

        stream.Position += 3;

        var symbols = new List<Symbol>();

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

            symbols.Add(new Symbol(symbolHeader, symbolRecord));
        }

        return new SymbolFile(header, version, targetUnit, symbols);
    }
}