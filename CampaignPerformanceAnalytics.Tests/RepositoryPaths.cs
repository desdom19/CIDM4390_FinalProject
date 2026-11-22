namespace CampaignPerformanceAnalytics.Tests;

internal static class RepositoryPaths
{
    /// <summary>The repository root: the nearest folder above the test binaries that holds GLOSSARY.md.</summary>
    public static string Root { get; } = FindRoot();

    public static string SampleData(string fileName) => Path.Combine(Root, "sample-data", fileName);

    private static string FindRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "GLOSSARY.md")))
            {
                return dir.FullName;
            }
        }
        throw new InvalidOperationException("Could not find the repository root (a folder holding GLOSSARY.md).");
    }
}
