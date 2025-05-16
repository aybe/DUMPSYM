using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM.Tests.WorkInProgress;

[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "CommentTypo")]
public sealed class SdkMatch
    // TODO version is 3.3 because of GsOBJTABLE and SpuStCallbackProc
{
    private const string KERNEL = "KERNEL.H";
    private const string TYPES  = "TYPES.H";
    private const string STDDEF = "STDDEF.H";
    private const string LIBGTE = "LIBGTE.H";
    private const string LIBGPU = "LIBGPU.H";
    private const string LIBGS  = "LIBGS.H";
    private const string LIBSPU = "LIBSPU.H";
    private const string LIBCD  = "LIBCD.H";

    public static ImmutableDictionary<string, string> DefinitionsMap { get; } = new Dictionary<string, string>
    {
        { "wchar_t", STDDEF },
        { "size_t", TYPES },
        { "u_char", TYPES },
        { "u_short", TYPES },
        { "u_long", TYPES },
        { "u_int", TYPES },
        { "ushort", TYPES },
        { "physadr", TYPES },
        { "label_t", TYPES },
        { "quad", TYPES },
        { "daddr_t", TYPES },
        { "caddr_t", TYPES },
        { "qaddr_t", TYPES },
        { "ino_t", TYPES },
        { "swblk_t", TYPES },
        { "time_t", TYPES },
        { "dev_t", TYPES },
        { "off_t", TYPES },
        { "uid_t", TYPES },
        { "gid_t", TYPES },
        { "MATRIX", LIBGTE },
        { "VECTOR", LIBGTE },
        { "SVECTOR", LIBGTE },
        { "CVECTOR", LIBGTE },
        { "DVECTOR", LIBGTE },
        { "EVECTOR", LIBGTE },
        { "RVECTOR", LIBGTE },
        { "CRVECTOR3", LIBGTE },
        { "DIVPOLYGON3", LIBGTE },
        { "CRVECTOR4", LIBGTE },
        { "DIVPOLYGON4", LIBGTE },
        { "SPOL", LIBGTE },
        { "POL4", LIBGTE },
        { "POL3", LIBGTE },
        { "RECT", LIBGPU },
        { "DR_ENV", LIBGPU },
        { "DRAWENV", LIBGPU },
        { "DISPENV", LIBGPU },
        { "P_TAG", LIBGPU },
        { "P_CODE", LIBGPU },
        { "POLY_F3", LIBGPU },
        { "POLY_F4", LIBGPU },
        { "POLY_FT3", LIBGPU },
        { "POLY_FT4", LIBGPU },
        { "POLY_G3", LIBGPU },
        { "POLY_G4", LIBGPU },
        { "POLY_GT3", LIBGPU },
        { "POLY_GT4", LIBGPU },
        { "LINE_F2", LIBGPU },
        { "LINE_G2", LIBGPU },
        { "LINE_F3", LIBGPU },
        { "LINE_G3", LIBGPU },
        { "LINE_F4", LIBGPU },
        { "LINE_G4", LIBGPU },
        { "BLK_FILL", LIBGPU }, // 2.0 -> 3.6
        { "SPRT", LIBGPU },
        { "SPRT_16", LIBGPU },
        { "SPRT_8", LIBGPU },
        { "TILE", LIBGPU },
        { "TILE_16", LIBGPU },
        { "TILE_8", LIBGPU },
        { "TILE_1", LIBGPU },
        { "DR_MODE", LIBGPU },
        { "DR_PRIO", LIBGPU }, // 2.6 -> 3.6
        { "DR_TWIN", LIBGPU },
        { "DR_AREA", LIBGPU },
        { "DR_OFFSET", LIBGPU },
        { "TMD_PRIM", LIBGPU },
        { "TIM_IMAGE", LIBGPU },
        { "PACKET", LIBGS },
        { "GsCOORDINATE", LIBGS }, // 2.0 -> 3.3
        { "GsCOORD2PARAM", LIBGS },
        { "GsCOORDINATE2", LIBGS },
        { "GsVIEW", LIBGS }, // 2.0 -> 3.3
        { "GsVIEW2", LIBGS },
        { "GsRVIEW", LIBGS }, // 2.0 -> 3.3
        { "GsRVIEW2", LIBGS },
        { "GsF_LIGHT", LIBGS },
        { "GsOT_TAG", LIBGS },
        { "GsOT", LIBGS },
        { "GsDOBJ", LIBGS }, // 2.0 -> 3.3
        { "GsDOBJ2", LIBGS },
        { "GsDOBJ3", LIBGS },
        { "GsDOBJ5", LIBGS },
        { "GsSPRITE", LIBGS },
        { "GsSPARRAY", LIBGS }, // 2.6 -> 4.1
        { "GsCELL", LIBGS },
        { "GsMAP", LIBGS },
        { "GsBG", LIBGS },
        { "GsLINE", LIBGS },
        { "GsGLINE", LIBGS },
        { "GsBOXF", LIBGS },
        { "GsFOGPARAM", LIBGS },
        { "GsIMAGE", LIBGS },
        { "_GsPOSITION", LIBGS },
        { "GsZCLIP", LIBGS },     // 2.0 -> 3.6 
        { "GsOBJTABLE", LIBGS },  // 2.0 -> 3.3
        { "GsOBJTABLE2", LIBGS }, // 2.0 -> 4.6
        { "GsMIMEV", LIBGS },     // 2.0 -> 3.6
        { "GsMIMEN", LIBGS },     // 2.0 -> 3.6 
        { "SpuVolume", LIBSPU },
        { "SpuVolume16", LIBSPU }, // 2.6 -> 3.5
        { "SpuVoiceAttr", LIBSPU },
        { "SpuReverbAttr", LIBSPU },
        { "SpuDecodeData", LIBSPU },
        { "SpuExtAttr", LIBSPU },
        { "SpuCommonAttr", LIBSPU },
        { "SpuIRQCallbackProc", LIBSPU },
        { "SpuTransferCallbackProc", LIBSPU },
        { "SpuStCallbackProc", LIBSPU }, // 3.3 -> 4.6
        { "CdlLOC", LIBCD },
        { "CdlFILTER", LIBCD },
        { "CdlATV", LIBCD },
        { "CdlFILE", LIBCD },
        { "StSECTOR", LIBCD }, // 2.0 -> 4.6
        { "StHEADER", LIBCD },
    }.ToImmutableDictionary();

    public static ImmutableDictionary<string, string> TypesMap { get; } = new Dictionary<string, string>
    {
        { "ToT", KERNEL },
        { "TCBH", KERNEL },
        { "TCB", KERNEL },
        { "EvCB", KERNEL },
        { "EXEC", KERNEL },
        { "XF_HDR", KERNEL },
        { "DIRENTRY", KERNEL },
        { "_GsCOORDINATE", LIBGS },
        { "_GsCOORDINATE2", LIBGS },
        { "_physadr", TYPES },
        { "_quad", TYPES },
        { "label_t", TYPES },
    }.ToImmutableDictionary();
}