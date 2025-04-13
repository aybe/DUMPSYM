namespace DUMPSYM;

[Serializable]
public sealed class SymbolRecordDef : SymbolRecord, ISymbolName, ISymbolDefinition
{
    public SymbolRecordDef()
    {
    }

    public SymbolRecordDef(SymbolContext context)
    {
        Class = context.Read<SymbolStorageClass>();
        Type = context.Read<SymbolType>();
        Size = context.Read<uint>();
        Name = context.ReadStringAscii();
    }

    public SymbolStorageClass Class { get; set; }

    public SymbolType Type { get; set; }

    public uint Size { get; set; }

    public string Name { get; set; } = null!;

    public override string ToString()
    {
        return $"Def class {Class} type {Type} size {Size} name {Name}";
    }
}