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
    
    internal Package()
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
        return string.IsNullOrWhiteSpace(PackageNugetVersion) ? PackageVersion :  PackageNugetVersion;
    }
    
    internal string GetVersionComparator()
    {
        return $"New {PackageNugetVersion ?? PackageVersion} <-> Older {PackageVersion}";
    }
    

    internal bool HasUpdate() =>
        !string.IsNullOrWhiteSpace(PackageNugetVersion) && PackageNugetVersion != PackageVersion;
}