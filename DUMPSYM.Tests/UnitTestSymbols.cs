namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestSymbols : UnitTestBase
{
    [TestMethod] // TODO dynamic data
    // ReSharper disable StringLiteralTypo
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\AFFECT.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\ANGLE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\BUILDING.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\CAMERA.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\CONTROL.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\DISTANCE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\DRAW.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\EFFECT.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\ENGINE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\FLOATLIB.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\FORLIB.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\GAME.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\GENMAP.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\LEVEL.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MAIN.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MAP.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MAPWHO.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MEMCARD.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MOVE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MUSIC.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\OBJECT.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\OBJECTS.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\OPTMENU.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\PACKET.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\PERSON.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\POWERUP.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\PSXHOST.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\PSXIO.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SCANNER.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SCREENS.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SEARCH.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SHOT.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SOUND.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SSPRITE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\STATS.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SUPER.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SWITCH.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\TEXT.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\THING.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\TRACK.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\VEHCOLID.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\VEHICLE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\WEAPON.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\WEATHER.C.json")]
    public void SplitAsArraysInFileOrder(string path)
    {
        var symbols = UnitTestSplit222.GetSymbols(path).ToArray(); // TODO extract method

        Symbol.Split(symbols);
    }
}