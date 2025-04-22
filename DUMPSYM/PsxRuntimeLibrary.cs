// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace DUMPSYM;

public static class PsxRuntimeLibrary
{
    public static Dictionary<string, string> Names { get; } = new()
    {
        { "_GsCOORDINATE", "GsCOORDINATE" },
        { "_GsCOORDINATE2", "GsCOORDINATE2" }
    };

    public static string[] Structures { get; } = new[]
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
        // Chapter 7: Basic Graphics Library
        "DISPENV",
        "DRAWENV",
        "DR_AREA",
        "DR_ENV",
        "DR_LOAD",
        "DR_MODE",
        "DR_MOVE",
        "DR_OFFSET",
        "DR_STP",
        "DR_TPAGE",
        "DR_TWIN",
        "LINE_F2",
        "LINE_F3",
        "LINE_F4",
        "LINE_G2",
        "LINE_G3",
        "LINE_G4",
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
        "SPRT_8",
        "SPRT_16",
        "TILE",
        "TILE_1",
        "TILE_8",
        "TILE_16",
        "TIM_IMAGE",
        "TMD_PRIM",
        // Chapter 8: Basic Geometry Library
        "CRVECTOR3",
        "CRVECTOR4",
        "CVECTOR",
        "DIVPOLYGON3",
        "DIVPOLYGON4",
        "DVECTOR",
        "EVECTOR",
        "MATRIX",
        "POL3",
        "POL4",
        "QMESH",
        "RVECTOR",
        "SPOL",
        "SVECTOR",
        "TMESH",
        "VECTOR",
        // Chapter 9: Extended Graphics Library
        "GsBG",
        "GsBOXF",
        "GsCELL",
        "GsCOORD2PARAM",
        "GsCOORDINATE2",
        "GsDOBJ2",
        "GsDOBJ3",
        "GsDOBJ5",
        "GsFOGPARAM",
        "GsF_LIGHT",
        "GsGLINE",
        "GsIMAGE",
        "GsLINE",
        "GsMAP",
        "GsOBJTABLE2",
        "GsOT",
        "GsOT_TAG",
        "GsRVIEW2",
        "GsSPRITE",
        "GsVIEW2",
        "TMD_STRUCT",
        "_GsFCALL",
        "_GsPOSITION",
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
        "sMcGuiTexture"
    }.OrderBy(s => s).ToArray();

    public static int StructuresPriority { get; } = Math.Min(-1000, -Structures.Length);
}