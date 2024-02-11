namespace Bornlogic.NugetDependabot.Entities;

public class NugetListResponse
{
    public long Count { get; set; }
    public IEnumerable<NugetItemResponse> Items { get; set; }
}