using JetBrains.Annotations;

namespace DUMPSYM.Symbols;

[NoReorder]
public sealed record SymbolRecordName : SymbolRecord, ISymbolVariable
{
    public SymbolRecordName(SymbolContext context)
    {
        Name = context.ReadStringAscii();
    }

    public string Name { get; set; } = null!;

    public override string ToString()
    {
        return $"{Name}";
    }
}