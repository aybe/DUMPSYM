using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM.Tests.WorkInProgress;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public static class Headers
{
    static Headers()
    {
        // TODO cleanup/sort

        TYPES = new Header("SYS/TYPES.H");

        FS = new Header("FS.H", TYPES);

        LIBPRESS = new Header("LIBPRESS.H", TYPES);

        MCGUI = new Header("MCGUI.H");

        SETJMP = new Header("SETJMP.H");

        STDARG = new Header("STDARG.H");

        STDDEF = new Header("STDDEF.H");

        KERNEL = new Header("KERNEL.H");

        LIBCD = new Header("LIBCD.H", TYPES);

        LIBDS = new Header("LIBDS.H", TYPES);

        LIBMCRD = new Header("LIBMCRD.H");

        LIBSND = new Header("LIBSND.H");

        LIBSPU = new Header("LIBSPU.H");

        LIBGTE = new Header("LIBGTE.H", TYPES);

        LIBGPU = new Header("LIBGPU.H", LIBGTE);

        LIBGS = new Header("LIBGS.H", LIBGPU);

        LIBHMD = new Header("LIBHMD.H", LIBGS);
    }

    public static Header FS { get; }

    public static Header TYPES { get; }

    public static Header KERNEL { get; }

    public static Header LIBCD { get; }

    public static Header LIBDS { get; }

    public static Header LIBGPU { get; }

    public static Header LIBGTE { get; }

    public static Header LIBGS { get; }

    public static Header LIBHMD { get; }

    public static Header LIBMCRD { get; }

    public static Header LIBPRESS { get; }

    public static Header LIBSND { get; }

    public static Header LIBSPU { get; }

    public static Header MCGUI { get; }

    public static Header SETJMP { get; }

    public static Header STDARG { get; }

    public static Header STDDEF { get; }
}