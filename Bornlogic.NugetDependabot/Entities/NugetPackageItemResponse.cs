using Newtonsoft.Json;

namespace Bornlogic.NugetDependabot.Entities;

public class NugetPackageItemResponse
{
    [JsonProperty("packageContent")]
    public string PackageContent { get; set; }
    public string Version { get; set; }
    [JsonProperty("catalogEntry")]
    public NugetPackageItemResponse CatalogEntry { get; set; }
}