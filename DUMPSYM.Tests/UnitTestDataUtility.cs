namespace DUMPSYM.Tests;

public static class UnitTestDataUtility // TODO move to library
{
    public static string GetFullPath(in string path)
    {
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
    }
}