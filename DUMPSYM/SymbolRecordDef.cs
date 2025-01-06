namespace DUMPSYM;

public sealed class SymbolRecordDef(SymbolContext context) : SymbolRecord, ISymbolName, ISymbolDefinition
{
    public SymbolStorageClass Class { get; } = context.Read<SymbolStorageClass>();

    public SymbolType Type { get; } = context.Read<SymbolType>();

    public uint Size { get; } = context.Read<uint>();

    public string Name { get; } = context.ReadStringAscii();

    public override string ToString()
    {
        return $"Def class {Class} type {Type} size {Size} name {Name}";
    }
}