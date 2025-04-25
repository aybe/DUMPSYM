using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM.Tests.WorkInProgress;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public static class Headers
{
    public static Header TYPES { get; } = new("SYS/TYPES.H");

    public static Header FS { get; } = new("FS.H", TYPES);

    public static Header KERNEL { get; } = new("KERNEL.H");

    public static Header LIBCD { get; } = new("LIBCD.H", TYPES);

    public static Header LIBDS { get; } = new("LIBDS.H", TYPES);

    public static Header LIBGTE { get; } = new("LIBGTE.H", TYPES);

    public static Header LIBGPU { get; } = new("LIBGPU.H", LIBGTE);

    public static Header LIBGS { get; } = new("LIBGS.H", LIBGPU);

    public static Header LIBHMD { get; } = new("LIBHMD.H", LIBGS);

    public static Header LIBMCRD { get; } = new("LIBMCRD.H");

    public static Header LIBPRESS { get; } = new("LIBPRESS.H", TYPES);

    public static Header LIBSND { get; } = new("LIBSND.H");

    public static Header LIBSPU { get; } = new("LIBSPU.H");

    public static Header MCGUI { get; } = new("MCGUI.H");

    public static Header SETJMP { get; } = new("SETJMP.H");

    public static Header STDARG { get; } = new("STDARG.H");

    public static Header STDDEF { get; } = new("STDDEF.H");
}