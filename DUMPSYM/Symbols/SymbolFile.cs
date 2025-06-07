using System.Collections;
using Whatever.Extensions;

namespace DUMPSYM.Symbols;

public sealed class SymbolFile : IEnumerable<SymbolRecord>
{
    private SymbolFile(string header, int version, int targetUnit, List<Symbol> symbols)
    {
        Header = header;
        Version = version;
        TargetUnit = targetUnit;
        Symbols = symbols;
    }

    public string Header { get; set; } = null!;

    public int Version { get; set; }

    public int TargetUnit { get; set; }

    public List<Symbol> Symbols { get; set; } = null!;

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

        var ctx = new SymbolContext(stream);

        while (stream.Position < stream.Length)
        {
            var hdr = new SymbolHeader(stream);

            ctx.Header = hdr;

            SymbolRecord rec = hdr.Type switch
            {
                0x01 => new SymbolRecordName(ctx),
                0x02 => new SymbolRecordName(ctx),
                0x06 => new SymbolRecordName(ctx),
                0x88 => new SymbolRecordSetSldToLineOfFile(ctx),
                0x82 => new SymbolRecordIncSldLineNumByByte(ctx),
                0x84 => new SymbolRecordIncSldLineNumByWord(ctx),
                0x80 => new SymbolRecordIncSldLineNum(ctx),
                0x86 => new SymbolRecordSetSldLineNum(ctx),
                0x8A => new SymbolRecordEndSldInfo(),
                0x8C => new SymbolRecordFunctionStart(ctx),
                0x8E => new SymbolRecordFunctionEnd(ctx),
                0x94 => new SymbolRecordDef(ctx),
                0x96 => new SymbolRecordDef2(ctx),
                0x98 => new SymbolRecordOverlay(ctx),
                0x90 => new SymbolRecordBlockStart(ctx),
                0x92 => new SymbolRecordBlockEnd(ctx),
                0x9A => new SymbolRecordSetOverlay(),
                0x9C => new SymbolRecordFunction2Start(ctx),
                _    => throw new NotImplementedException($"0x{hdr.Type:x2} @ {stream.Position - 5:X8}")
            };

            symbols.Add(new Symbol(hdr, rec));
        }

        return new SymbolFile(header, version, targetUnit, symbols);
    }
}