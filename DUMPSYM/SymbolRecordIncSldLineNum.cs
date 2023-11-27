namespace DUMPSYM;

public sealed class SymbolRecordIncSldLineNum : SymbolRecord
{
    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Inc SLD linenum (to {line})");
    }
}