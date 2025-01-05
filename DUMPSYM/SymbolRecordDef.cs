namespace DUMPSYM;

public sealed class SymbolRecordDef(SymbolContext context) : SymbolRecord, ISymbolName, ISymbolDefinition
{
    public SymbolStorageClass Class { get; } = context.Read<SymbolStorageClass>();

    public SymbolType Type { get; } = context.Read<SymbolType>();

    public uint Size { get; } = context.Read<uint>();

    uint[] ISymbolDefinition.Dimensions => []; // TODO see if there's a better way to do this

    string ISymbolDefinition.Tag => string.Empty; // TODO see if there's a better way to do this

    public string Name { get; } = context.ReadStringAscii();

    public override string ToString()
    {
        return $"Def class {Class} type {Type} size {Size} name {Name}";
    }
}