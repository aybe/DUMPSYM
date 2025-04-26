using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace DUMPSYM.Tests.WorkInProgress;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed record PsxSdk(string Name, Version Version, PsxSdkTag[] Tags)
{
    public static PsxSdk Version20Japan { get; } = new(
        "Runtime Library Version 2.0 (Japan)",
        new Version(2, 0),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "cb8faaf7082438555a4d32625ffbb72c807e8ff11f0f4ec37440e1c2fc458bc3"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "f92340ecb71854e3d7dbe1150a2e7200154d38c95a8f85ce39d87e9a35fc55a3")
        ]);

    public static PsxSdk Version26Japan { get; } = new(
        "Runtime Library Version 2.6 (Japan)",
        new Version(2, 6),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "9f9904b295c3455e1486c7f406a426c400a640832ecebfc18c23adc41e456371"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "a55dc74d369782fc23c8ea3001dee34f3339dae44f2c6f1870720f08ae66e62e")
        ]);

    public static PsxSdk Version30Japan { get; } = new(
        "Runtime Library Version 3.0 (Japan)",
        new Version(3, 0),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "1396280d4a5eac383f486c6b9384d73fb64f954160651b90f2bd61129d56ed69"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "5bac11333bed070adb473e4755782ee91f7680e661794b48ce43d970014eaee2")
        ]);

    public static PsxSdk Version33Japan { get; } = new(
        "Runtime Library Version 3.3 (Japan)",
        new Version(3, 3),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "8390de7801c9e11ceb145e2efec638ce771005de154fe6ff17d9d871acb6f97f"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "b3f18aae45e24aeadcaf12ece46abdbc8f9098ec5a7077e3e7c2ff884078d86b")
        ]);

    public static PsxSdk Version35Japan { get; } = new(
        "Runtime Library Version 3.5 (Japan)",
        new Version(3, 5),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "408ee6d550e18bd6169a9c2aebf4026cff8faa98ceb0b14354ded8e371db7d45"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "2091b3d274941499adc8b955baf4536f34fbf0013e3633bd26d3da38c368c123")
        ]);

    public static PsxSdk Version36Japan { get; } = new(
        "Runtime Library Version 3.6 (Japan)",
        new Version(3, 6),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "390625480a235b55c7e5662963cb6f6b2379a9fd10852e42e77dd9aa99be57b0"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "13f35a09d9289f68649555314e0abc479fab62d2ab002c93383994bcdbfa9f9e")
        ]);

    public static PsxSdk Version40Japan { get; } = new(
        "Runtime Library Version 4.0 (Japan)",
        new Version(4, 0),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "bd13d8375fa5f13117aca503ae1af0a236c5f0c90a1105ea291831cd5c36c162"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "21cf985e3db9b9ea20f7f8ce75ad79683496b20e16e235e908a2652439253646")
        ]);

    public static PsxSdk Version41Japan { get; } = new(
        "Runtime Library Version 4.1 (Japan)",
        new Version(4, 1),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "4384ba1c911102fda125b2ff5db3e196e27b5207d1ece6125695cd0f72c5503d"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "fd7edd5643c361eee91abbd3e3645e4a0ebd2a16cb7be409d35d7a099da4b524")
        ]);

    public static PsxSdk Version43Japan { get; } = new(
        "Runtime Library Version 4.3 (Japan)",
        new Version(4, 3),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "e10e42368be70ecf6b304bb4ca2f2e5a1818b994f30482e5061ca5a6ccee95e3"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "01004e27dfbddf626e75f03833f6c187712c773627548580da3647ebca84cbae")
        ]);

    public static PsxSdk Version44Japan { get; } = new(
        "Runtime Library Version 4.4 (Japan)",
        new Version(4, 4),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "570c094691dbddcce8aab8e50e0e87552464cb43ab07c5f2a33a8dd4899b822c"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "73f3f0935d0191f3fbb94541de19e7e5b4532f3e492c6261e97295fd60ef1e38")
        ]);

    public static PsxSdk Version46Japan { get; } = new(
        "Runtime Library Version 4.6 (Japan)",
        new Version(4, 6),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "58161767a34aa41e2c4647316a56da4acd75c3a6d6b77d9ceebbdd4bd81107ac"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "f2a6591d024fd1f1abc7a89e1a3aef611510694109340fae5fe8995937d16cea")
        ]);

    public static PsxSdk Version47Unknown { get; } = new(
        "Runtime Library Version 4.7 (Unknown)",
        new Version(4, 7),
        [
            new PsxSdkTag("INCLUDE/LIBGPU.H", "a77997bb6b247d339f45fbd340794e3a3621e09e7915f2303f59e40f7834cc17"),
            new PsxSdkTag("INCLUDE/LIBGTE.H", "bd5590f283bfdb4c6d30c59794bdae96a7ac49fe4f404b185b6938011a238dff")
        ]);

    public static ImmutableArray<PsxSdk> Versions { get; } =
    [
        Version20Japan,
        Version26Japan,
        Version30Japan,
        Version33Japan,
        Version35Japan,
        Version36Japan,
        Version40Japan,
        Version41Japan,
        Version43Japan,
        Version44Japan,
        Version46Japan,
        Version47Unknown
    ];

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Version)}: {Version}";
    }

    public static PsxSdk? TryIdentify(string directory)
    {
        var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Select(GetPath).ToArray();

        var hashes = new ConcurrentDictionary<string, string>();

        foreach (var sdk in Versions)
        {
            foreach (var tag in sdk.Tags)
            {
                var path = GetPath(tag.Path);

                foreach (var file in files.Where(s => s.EndsWith(path, StringComparison.OrdinalIgnoreCase)))
                {
                    if (string.Equals(hashes.GetOrAdd(file, GetHash), tag.Hash, StringComparison.OrdinalIgnoreCase))
                    {
                        return sdk;
                    }
                }
            }
        }

        return null;

        static string GetPath(string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        static string GetHash(string path)
        {
            return Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));
        }
    }
}