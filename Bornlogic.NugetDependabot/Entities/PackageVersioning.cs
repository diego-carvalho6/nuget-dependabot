namespace Bornlogic.NugetDependabot.Entities;

internal class PackageVersioning
{
    private const int DefaultVersionMultiplier = 10;
    internal int? MajorVersion { get; set; }
    internal int? MinorVersion { get; set; }
    internal int? PatchVersion { get; set; }
    internal int? ParseVersion { get; set; }
    internal string Version { get; set; }
    
    internal PackageVersioning(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return;
        
        var splitedValues = version.Split(Constants.DefaultVersionSeparator);

        MajorVersion = int.TryParse(splitedValues.ElementAtOrDefault(0), out var firstValue) ? firstValue : null;
        MinorVersion = int.TryParse(splitedValues.ElementAtOrDefault(1), out var secondValue) ? secondValue : null;
        PatchVersion = int.TryParse(splitedValues.ElementAtOrDefault(2), out var thirdValue) ? thirdValue : null;
        Version = version;
        var parseVersion = MajorVersion;
        
        if (MinorVersion != null)
            parseVersion *= DefaultVersionMultiplier + MinorVersion;
        
        if (PatchVersion != null)
            parseVersion *= DefaultVersionMultiplier + PatchVersion;

        ParseVersion = parseVersion;

    }
}