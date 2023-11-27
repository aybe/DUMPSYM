namespace DUMPSYM;

public sealed class SymbolRecordSetOverlay : SymbolRecord
{
    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddress(writer);

        writer.WriteLine("set overlay");
    }
}