using Newtonsoft.Json;

namespace Bornlogic.NugetDependabot.Entities;

public class NugetItemResponse
{
    public string Lower { get; set; }
    [JsonProperty("@type")]
    public string Type { get; set; }
    public string Upper { get; set; }
    public long Count { get; set; }
    public IEnumerable<NugetItemResponse> Items { get; set; }
}