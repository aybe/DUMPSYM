namespace DUMPSYM;

public sealed class SymbolRecordDef : SymbolRecord, ISymbolName, ISymbolDefinition
{
    public SymbolRecordDef(SymbolContext context) : base(context)
    {
        Class = context.Read<SymbolStorageClass>();
        Type = context.Read<SymbolType>();
        Size = context.Read<uint>();
        Name = context.ReadStringAscii();
    }

    public SymbolStorageClass Class { get; }

    public SymbolType Type { get; }

    public uint Size { get; }

    uint[] ISymbolDefinition.Dimensions => []; // TODO see if there's a better way to do this

    string ISymbolDefinition.Tag => string.Empty; // TODO see if there's a better way to do this

    public string Name { get; }

    public override string ToString()
    {
        return $"Def class {Class} type {Type} size {Size} name {Name}";
    }
}