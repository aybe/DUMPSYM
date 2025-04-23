// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace DUMPSYM;

public static class PsxRuntimeLibrary
{
    public static Dictionary<string, string> Names { get; } = new()
    {
        { "_GsCOORDINATE", "GsCOORDINATE" },
        { "_GsCOORDINATE2", "GsCOORDINATE2" },
        { "_GsPOSITION", "GsPOSITION" }
    };

    public static string[] Structures { get; } = new[] // TODO finish this up, it will take some time
    {
        // Chapter 1: Kernel Library
        "DIRENTRY",
        "EvCB",
        "EXEC",
        "TCB",
        "TCBH",
        "ToT",
        // Chapter 2: Standard C Library
        // NONE
        // Chapter 3: Math Library
        // NONE
        // Chapter 4: Memory Card Library
        // NONE
        // Chapter 5: Extended Memory Card Library
        // NONE
        // Chapter 6: Data Compression Library
        "DECDCTENV",
        "ENCSPUENV",
        // Chapter 7: Basic Graphics Library (LIBGPU.H)
        "BLK_FILL", // SDK <= 3.6
        "DISPENV",
        "DR_AREA",
        "DR_ENV",
        "DR_LOAD", // SDK >= 3.5
        "DR_MODE",
        "DR_MOVE", // SDK >= 3.3
        "DR_OFFSET",
        "DR_PRIO", // SDK <= 3.6
        "DR_STP", // SDK >= 4.1
        "DR_TPAGE", // SDK >= 3.5
        "DR_TWIN",
        "DRAWENV",
        "LINE_F2",
        "LINE_F3",
        "LINE_F4",
        "LINE_G2",
        "LINE_G3",
        "LINE_G4",
        "P_CODE",
        "P_TAG",
        "POLY_F3",
        "POLY_F4",
        "POLY_FT3",
        "POLY_FT4",
        "POLY_G3",
        "POLY_G4",
        "POLY_GT3",
        "POLY_GT4",
        "RECT",
        "RECT32",
        "SPRT",
        "SPRT_16",
        "SPRT_8",
        "TILE",
        "TILE_1",
        "TILE_16",
        "TILE_8",
        "TIM_IMAGE",
        "TMD_PRIM",
        // Chapter 8: Basic Geometry Library (LIBGTE.H)
        "CRVECTOR3",
        "CRVECTOR4",
        "CVECTOR",
        "DIVPOLYGON3", // SDK >= 2.6
        "DIVPOLYGON4", // SDK >= 2.6
        "DVECTOR",
        "EVECTOR",
        "MATRIX",
        "POL3", // SDK >= 2.6
        "POL4", // SDK >= 2.6
        "QMESH", // SDK >= 3.3
        "RVECTOR", // SDK >= 2.6
        "SPOL", // SDK >= 2.6
        "SVECTOR",
        "TMESH", // SDK >= 3.3
        "VECTOR",
        // Chapter 9: Extended Graphics Library (LIBGS.H)
        "_GsFCALL", // SDK >= 3.3
        "_GsPOSITION",
        "GsBG",
        "GsBOXF",
        "GsCELL",
        "GsCOORD2PARAM",
        "GsCOORDINATE",
        "GsCOORDINATE2",
        "GsDOBJ",
        "GsDOBJ2",
        "GsDOBJ3",
        "GsDOBJ5",
        "GsF_LIGHT",
        "GsFOGPARAM",
        "GsGLINE",
        "GsIMAGE",
        "GsLINE",
        "GsMAP",
        "GsMIMEN", // SDK <= 3.6
        "GsMIMEV", // SDK >= 3.6
        "GsOBJTABLE", // SDK <= 3.3
        "GsOBJTABLE2",
        "GsOT",
        "GsOT_TAG",
        "GsRVIEW",
        "GsRVIEW2",
        "GsSPARRAY", // SDK >= 2.6 <= 4.1
        "GsSPRITE",
        "GsVIEW",
        "GsVIEW2",
        "GsZCLIP", // SDK >= 3.6
        "TMD_STRUCT", // SDK >= 3.3
        // Chapter 10: CD/Streaming Library
        "CdlATV",
        "CdlFILE",
        "CdlFILTER",
        "CdlLOC",
        "StHEADER",
        // Chapter 11: Extended CD-ROM Library
        "DslATV",
        "DslFILE",
        "DslFILTER",
        "DslLOC",
        // Chapter 12: Controller/Peripherals Library
        // NONE
        // Chapter 13: Link Cable Library
        // NONE
        // Chapter 14: Extended Sound Library
        "ProgAtr",
        "SndRegisterAttr",
        "SndVoiceStats",
        "SndVolume",
        "SndVolume2",
        "VabHdr",
        "VagAtr",
        "_SsFCALL",
        // Chapter 15: Basic Sound Library
        "SpuCommonAttr",
        "SpuDecodeData",
        "SpuEnv",
        "SpuExtAttr",
        "SpuLVoiceAttr",
        "SpuReverbAttr",
        "SpuStEnv",
        "SpuStVoiceAttr",
        "SpuVoiceAttr",
        "SpuVolume",
        // Chapter 16: Serial Input/Output Library
        // NONE
        // Chapter 17: HMD Library
        "GsARGUNIT",
        "GsARGUNIT_ANIM",
        "GsARGUNIT_GND",
        "GsARGUNIT_GNDT",
        "GsARGUNIT_IMAGE",
        "GsARGUNIT_JntMIMe",
        "GsARGUNIT_NORMAL",
        "GsARGUNIT_RstJntMIMe",
        "GsARGUNIT_RstVNMIMe",
        "GsARGUNIT_SHARED",
        "GsARGUNIT_VNMIMe",
        "GsCOORDUNIT",
        "GsRVIEWUNIT",
        "GsSEH",
        "GsSEQ",
        "GsTYPEUNIT",
        "GsUNIT",
        "GsVIEWUNIT",
        "GsWORKUNIT",
        // Chapter 18: PDA Library (libmcx)
        // NONE
        // Chapter 19:Memory Card GUI Module (mcgui)
        "McGuiEnv",
        "sMcGuiBg",
        "sMcGuiCards",
        "sMcGuiController",
        "sMcGuiCursor",
        "sMcGuiSnd",
        "sMcGuiTexture",
        // KERNEL.H
        "XF_HDR",
        // LIBCD.H
        "StSECTOR", // SDK >= 2.6 <= 3.0
        // LIBSPU.H
        "SpuVolume16" // SDK >= 2.6 <= 3.5
    }.OrderBy(s => s).ToArray();

    public static string[] Types { get; } = // TODO structures shall be cleaned up from new ones in here
    [
        // LIBGS.H
        "GsCOORDINATE", // SDK >= 2.0 <= 3.3
        "GsCOORD2PARAM", // SDK >= 2.0 <= 4.6
        "GsCOORDINATE2", // SDK >= 2.0 <= 4.6
        "GsVIEW", // SDK >= 2.0 <= 3.3
        "GsVIEW2", // SDK >= 2.0 <= 4.6
        "GsRVIEW", // SDK >= 2.0 <= 3.3
        "GsRVIEW2", // SDK >= 2.0 <= 4.6
        "GsF_LIGHT", // SDK >= 2.0 <= 4.6
        "GsOT_TAG", // SDK >= 2.0 <= 4.6
        "GsOT", // SDK >= 2.0 <= 4.6
        "GsDOBJ", // SDK >= 2.0 <= 3.3
        "GsDOBJ2", // SDK >= 2.0 <= 4.6
        "GsDOBJ3", // SDK >= 2.0 <= 4.6
        "GsDOBJ5", // SDK >= 2.0 <= 4.6
        "GsSPRITE", // SDK >= 2.0 <= 4.6
        "GsSPARRAY", // SDK >= 2.6 <= 4.1
        "GsCELL", // SDK >= 2.0 <= 4.6
        "GsMAP", // SDK >= 2.0 <= 4.6
        "GsBG", // SDK >= 2.0 <= 4.6
        "GsLINE", // SDK >= 2.0 <= 4.6
        "GsGLINE", // SDK >= 2.0 <= 4.6
        "GsBOXF", // SDK >= 2.0 <= 4.6
        "GsFOGPARAM", // SDK >= 2.0 <= 4.6
        "GsIMAGE", // SDK >= 2.0 <= 4.6
        "_GsPOSITION", // SDK >= 2.0 <= 4.6
        "GsZCLIP", // SDK >= 2.0 <= 3.6
        "GsOBJTABLE", // SDK >= 2.0 <= 3.3
        "GsOBJTABLE2", // SDK >= 2.0 <= 4.6
        "GsMIMEV", // SDK >= 2.0 <= 3.6
        "GsMIMEN", // SDK >= 2.0 <= 3.6
        "_GsFCALL", // SDK >= 3.3 <= 4.6
        // LIBSPU.H // TODO more
        "SpuIRQCallbackProc", // SDK >= 2.6 <= 4.6
        "SpuTransferCallbackProc", // SDK >= 3.0 <= 4.6
        "SpuStCallbackProc", // SDK >= 3.3 <= 4.6
        // TYPES.H 4.6
        "u_char",
        "u_short",
        "u_int",
        "u_long",
        "ushort",
        "uint",
        "ulong",
        "physadr",
        "label_t",
        "quad",
        "daddr_t",
        "caddr_t",
        "qaddr_t",
        "ino_t",
        "swblk_t",
        "size_t",
        "time_t",
        "dev_t",
        "off_t",
        "gid_t",
        "uid_t"
    ];
}