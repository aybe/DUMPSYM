namespace DUMPSYM;

public sealed class SymbolRecordName(Stream stream) : SymbolRecord
{
    public string Name { get; } = SymbolUtility.ReadStringAscii(stream);

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"{Name}");
    }
}