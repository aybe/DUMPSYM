using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM.Symbols;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum SymbolTypeModifier
{
    PTR = 1,
    FCN = 2,
    ARY = 3
}