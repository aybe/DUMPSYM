using System.Diagnostics;
using JetBrains.Annotations;

namespace DUMPSYM.Tests;

public partial class UnitTestAuto
{
    [PublicAPI]
    public required TestContext TestContext { get; set; }

    private void Test(string symPath)
    {
        var txtPath = $"{symPath}.TXT";

        if (!File.Exists(txtPath))
        {
            using var process = new Process();

            var exePath = Path.GetFullPath(Path.Combine(TestContext.TestRunDirectory!, @"..\..\..\0\t\dumpsym.Tests\bin\Debug\net7.0\dumpsym.exe"));

            process.StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"\"{symPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            if (process.Start())
            {
                var output = process.StandardOutput.ReadToEnd();

                File.WriteAllText(txtPath, output);

                process.WaitForExit(TimeSpan.FromSeconds(5));
            }
        }

        // var txt = File.ReadAllText(txtPath);
        // 
        // TestContext.WriteLine(txt);

        UnitTest2.CompareDumps(symPath, txtPath);
    }
}