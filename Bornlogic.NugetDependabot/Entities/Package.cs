namespace Bornlogic.NugetDependabot.Entities;

public class Package
{
    private const string PackageTagOpening = "<PackageReference";
    private const string PackageTagClosing = "/>";
    private const string PackageTagInclude = "Include=";
    private const string PackageTagVersion = "Version=";
    
    private List<string> PackageReferences { get; set; }
    private string PackageName { get; set; }
    private string PackageVersion { get; set; }
    private string PackageNugetVersion { get; set; }

    
    private Package()
    {
        
    }

    internal string ToNugetPackageReference() => $"{PackageTagOpening} {PackageTagInclude}\u0022{PackageName}\u0022 {PackageTagVersion}\u0022{GetVersionByConfiguration()}\u0022 {PackageTagClosing}";
    
    
    internal Package(string packageReference, string packageName, string packageVersion, string packageNugetVersion)
    {
        PackageReferences = new List<string> { packageReference };
        PackageName = packageName;
        PackageVersion = packageVersion;
        PackageNugetVersion = packageNugetVersion;
    }

    internal void UpdatePackageReferences(string newReference) => PackageReferences = PackageReferences.Append(newReference).Distinct().ToList();
    internal IEnumerable<string> GetSavedPackageReferences() => PackageReferences;
    internal string GetPackageName() => PackageName;
    private string GetVersionByConfiguration()
    {
        // todo introduce custom configurations, example, update only minor
        
        return PackageNugetVersion ?? PackageVersion;
        
    }
}