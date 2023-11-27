namespace DUMPSYM;

public sealed class SymbolFile(string header, int version, int targetUnit, IList<Symbol> symbols)
{
    public string Header { get; } = header;

    public int Version { get; } = version;

    public int TargetUnit { get; } = targetUnit;

    public IList<Symbol> Symbols { get; } = symbols;

    public override string ToString()
    {
        using var writer = new StringWriter();

        writer.WriteLine();
        writer.WriteLine($"Header : {Header} version {Version}");
        writer.WriteLine($"Target unit {TargetUnit}");

        var line = 0u;

        foreach (var symbol in Symbols)
        {
            var header = symbol.Header;

            var record = symbol.Record;

            switch (record)
            {
                case SymbolRecordIncSldLineNum:
                    line += 1;
                    break;
                case SymbolRecordIncSldLineNumByByte a:
                    line += a.Length;
                    break;
                case SymbolRecordIncSldLineNumByWord b:
                    line += b.Length;
                    break;
                case SymbolRecordSetSldLineNum c:
                    line = c.Value;
                    break;
                case SymbolRecordSetSldToLineOfFile d:
                    line = d.Line;
                    break;
            }

            record.Write(header, writer, line);
        }

        var result = writer.ToString();

        return result;
    }
}