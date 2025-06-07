namespace DUMPSYM.Symbols;

public sealed record SymbolRecordOverlay : SymbolRecord, ISymbolOverlay
{
    public SymbolRecordOverlay(SymbolContext context)
    {
        Length = context.Read<uint>();
        Id = context.Read<uint>();
    }

    public uint Length { get; set; }

    public uint Id { get; set; }
}