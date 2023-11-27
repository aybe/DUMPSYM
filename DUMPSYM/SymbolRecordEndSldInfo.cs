namespace DUMPSYM;

public sealed class SymbolRecordEndSldInfo : SymbolRecord
{
    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine("End SLD info");
    }
}