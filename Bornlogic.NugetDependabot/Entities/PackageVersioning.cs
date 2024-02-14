namespace Bornlogic.NugetDependabot.Entities;

internal class PackageVersioning
{
    private const decimal DefaultVersionMinorDivider = 10000;
    private const decimal DefaultVersionPatchDivider = 1000000000;
    internal int? MajorVersion { get; set; }
    internal int? MinorVersion { get; set; }
    internal int? PatchVersion { get; set; }
    internal decimal? AllParsedVersion { get; set; }
    internal string Version { get; set; }
    
    internal PackageVersioning(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return;
        
        var splitedValues = version.Split(Constants.DefaultVersionSeparator);

        var thirdValueString = splitedValues.ElementAtOrDefault(2)?.Split(Constants.DefaultSubVersionSeparator)?.FirstOrDefault();
        
        MajorVersion = int.TryParse(splitedValues.ElementAtOrDefault(0), out var firstValue) ? firstValue : null;
        MinorVersion = int.TryParse(splitedValues.ElementAtOrDefault(1), out var secondValue) ? secondValue : null;
        PatchVersion = int.TryParse(thirdValueString, out var thirdValue) ? thirdValue : null;
        Version = version;
        AllParsedVersion = MajorVersion;
        
        if (MinorVersion != null)
            AllParsedVersion +=  MinorVersion != default(decimal) ? (decimal)MinorVersion / DefaultVersionMinorDivider : default(decimal);
        
        if (PatchVersion != null)
            AllParsedVersion +=  PatchVersion != default(decimal) ? (decimal)PatchVersion / DefaultVersionPatchDivider : default(decimal) ;

    }

    public bool PackageHasSubVersion() =>
        !string.IsNullOrWhiteSpace(Version?.Split(Constants.DefaultSubVersionSeparator)?.ElementAtOrDefault(1) );
}