using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public enum SymbolTypeKind : ushort
{
    NULL   = 0x0,
    VOID   = 0x1,
    CHAR   = 0x2,
    SHORT  = 0x3,
    INT    = 0x4,
    LONG   = 0x5,
    FLOAT  = 0x6,
    DOUBLE = 0x7,
    STRUCT = 0x8,
    UNION  = 0x9,
    ENUM   = 0xA,
    MOE    = 0xB,
    UCHAR  = 0xC,
    USHORT = 0xD,
    UINT   = 0xE,
    ULONG  = 0xF
}