namespace DUMPSYM.Symbols;

public sealed record SymbolRecordDef(SymbolStorageClass Class, SymbolType Type, uint Size, string Name)
    : SymbolRecord, ISymbolDefinition
{
    public SymbolRecordDef(SymbolContext context)
        : this(context.Read<SymbolStorageClass>(), context.Read<SymbolType>(), context.Read<uint>(), context.ReadStringAscii())
    {
    }

    public override string ToString()
    {
        return $"Def class {Class} type {Type} size {Size} name {Name}";
    }
}