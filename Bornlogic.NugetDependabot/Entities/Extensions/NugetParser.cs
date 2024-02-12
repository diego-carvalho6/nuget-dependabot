namespace Bornlogic.NugetDependabot.Entities.Extensions;

public static class NugetParser
{
    
    public static string ReturnGreater(IEnumerable<string> versions)
    {
        return versions?.MaxBy(x =>  new PackageVersioning(x).ParsedVersion);
    }
}