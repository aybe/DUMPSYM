namespace DUMPSYM;

public sealed class SymbolRecordOverlay(SymbolContext context) : SymbolRecord, ISymbolOverlay
{
    public uint Length { get; } = context.Read<uint>();

    public uint Id { get; } = context.Read<uint>();
}