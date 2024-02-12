namespace Bornlogic.NugetDependabot.Entities;

public static class NugetParser
{
    
    public static string ReturnGreater(IEnumerable<string> versions)
    {
        return versions?.MaxBy(x =>  new PackageVersioning(x).ParseVersion);
    }
}