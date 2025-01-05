using System.Collections;
using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolFile : IEnumerable<SymbolRecord>
{
    public SymbolFile(string header, int version, int targetUnit, IList<Symbol> symbols)
    {
        Header = header;
        Version = version;
        TargetUnit = targetUnit;
        Symbols = symbols;
    }

    public string Header { get; }

    public int Version { get; }

    public int TargetUnit { get; }

    public IList<Symbol> Symbols { get; }

    public IEnumerator<SymbolRecord> GetEnumerator()
    {
        return Symbols.Select(s => s.Record).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        using var writer = new StringWriter();

        writer.WriteLine();
        writer.WriteLine($"Header : {Header} version {Version}");
        writer.WriteLine($"Target unit {TargetUnit}");

        foreach (var symbol in Symbols)
        {
            writer.WriteLine(symbol.ToString());
        }

        var result = writer.ToString();

        return result;
    }

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

        var context = new SymbolContext(stream);

        while (stream.Position < stream.Length)
        {
            var symbolPosition = stream.Position;

            var symbolHeader = new SymbolHeader(stream);

            context.Header = symbolHeader;

            SymbolRecord symbolRecord = symbolHeader.Type switch
            {
                0x01 => new SymbolRecordName(context),
                0x02 => new SymbolRecordName(context),
                0x06 => new SymbolRecordName(context),
                0x88 => new SymbolRecordSetSldToLineOfFile(context),
                0x82 => new SymbolRecordIncSldLineNumByByte(context),
                0x84 => new SymbolRecordIncSldLineNumByWord(context),
                0x80 => new SymbolRecordIncSldLineNum(context),
                0x86 => new SymbolRecordSetSldLineNum(context),
                0x8A => new SymbolRecordEndSldInfo(context),
                0x8C => new SymbolRecordFunctionStart(context),
                0x8E => new SymbolRecordFunctionEnd(context),
                0x94 => new SymbolRecordDef(context),
                0x96 => new SymbolRecordDef2(context),
                0x98 => new SymbolRecordOverlay(context),
                0x90 => new SymbolRecordBlockStart(context),
                0x92 => new SymbolRecordBlockEnd(context),
                0x9A => new SymbolRecordSetOverlay(context),
                0x9C => new SymbolRecordFunction2Start(context),
                _ => throw new NotImplementedException($"0x{symbolHeader.Type:x2} @ {symbolPosition:X8}")
            };

            var symbol = new Symbol(symbolHeader, symbolRecord);

            symbols.Add(symbol);
        }

        return new SymbolFile(header, version, targetUnit, symbols);
    }
}