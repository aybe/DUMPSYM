using System.Diagnostics.CodeAnalysis;

namespace psx_dump_sym;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum SymbolTypeModifier
{
    PTR = 1,
    FCN = 2,
    ARY = 3
}