using System.Diagnostics.CodeAnalysis;

namespace psx_dump_sym;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public enum SymbolDefClass : ushort
{
    MemberOfStruct = 0x8,
    LABEL          = 0x6,
    EndOfStruct    = 0x66,
    AUTO           = 0x1,
    REGPARM        = 0x11,
    REG            = 0x4,
    ARG            = 0x9,
    STAT           = 0x3,
    TPDEF          = 0xD,
    STRTAG         = 0xA,
    UNTAG          = 0xC,
    MOU            = 0xB,
    FIELD          = 0x12,
    EXT            = 0x2
}