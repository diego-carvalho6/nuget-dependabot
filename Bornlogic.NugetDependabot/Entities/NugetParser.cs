using Bornlogic.NugetDependabot.Entities.Enums;

namespace Bornlogic.NugetDependabot.Entities;

internal static class NugetParser
{
    
    private static string ReturnGreater(IEnumerable<string> versions, Func<PackageVersioning, bool> filterFunc = null)
    {
        return versions?.Where(x => filterFunc != null && filterFunc(new PackageVersioning(x))).MaxBy(x => new PackageVersioning(x).AllParsedVersion);
    }
    
    internal static string GetVersionByNugetType(NugetListResponse response, string currentPackageVersion, NugetVersionUpdateType? type)
    {
        if (type == null)
            return ReturnGreater(response?.Items?.Select(x => x?.Upper));

        var versions = response?.Items?.SelectMany(x => x?.Items ?? new List<NugetPackageItemResponse>())?.ToList() ??
                       new List<NugetPackageItemResponse>();
        
        var currentVersioning = new PackageVersioning(currentPackageVersion);
        
        return ReturnGreater(versions.Select(x => x.Version ?? x.CatalogEntry?.Version), versioning =>
        {
            var validations = new List<bool>();
            
            if (type == NugetVersionUpdateType.All)
                return true;
            
            validations.Add(!versioning.PackageHasSubVersion());
            
            if (type == NugetVersionUpdateType.Major)
                validations.Add(versioning.MajorVersion >= currentVersioning.MajorVersion);
            else
                validations.Add(versioning.MajorVersion == currentVersioning.MajorVersion);

            if (type == NugetVersionUpdateType.Minor)
                validations.Add(versioning.MinorVersion >= currentVersioning.MinorVersion);
            
            if (type ==  NugetVersionUpdateType.Patch)
                validations.Add(versioning.PatchVersion >= currentVersioning.PatchVersion && versioning.MinorVersion == currentVersioning.MinorVersion);
            
            return validations.All(x => x);
        });
    }
    
    
}