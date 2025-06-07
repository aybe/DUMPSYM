using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM.Symbols;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public enum SymbolStorageClass : ushort
{
    AUTO    = 0x01,
    EXT     = 0x02,
    STAT    = 0x03,
    REG     = 0x04,
    LABEL   = 0x06,
    MOS     = 0x08,
    ARG     = 0x09,
    STRTAG  = 0x0A,
    MOU     = 0x0B,
    UNTAG   = 0x0C,
    TPDEF   = 0x0D,
    ENTAG   = 0x0F,
    MOE     = 0x10,
    REGPARM = 0x11,
    FIELD   = 0x12,
    EOS     = 0x66,
    FILE    = 0x67,
}