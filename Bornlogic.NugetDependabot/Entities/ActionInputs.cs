using CommandLine;

namespace Bornlogic.NugetDependabot.Entities;

public class ActionInputs
{
    private string _nugetSource;
    private string _allowedSources;
    
    [Option('n', "nuget-source",
        Required = false,
        HelpText = "The Nuget Source To Override, for example: \"https://api.nuget.org/v3/octorat/index.json\".")]
    public string NugetSource
    {
        get => _nugetSource;
        set => ParseAndAssign(value, str => _nugetSource = str);
    }
    [Option('a', "allowed-sources",
        Required = false,
        HelpText = "The Nuget Packages Allowed Split By | , for example: \"microsoft*|MarkdownBuilder|aws-sdk*\".")]
    public string AllowedSources
    {
        get => _allowedSources;
        set => ParseAndAssign(value, str => _allowedSources = str);
    }
    [Option('w', "workdir",
        Required = true,
        HelpText = "The workspace directory, or repository root directory.")]
    public string WorkspaceDirectory { get; set; } = null!;
    
    static void ParseAndAssign(string? value, Action<string> assign)
    {
        if (value is { Length: > 0 } && assign is not null)
        {
            assign(value.Split("/")[^1]);
        }
    }
    
    public ActionInputs()
    {
    }
}