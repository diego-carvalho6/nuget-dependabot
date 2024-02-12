using CommandLine;

namespace Bornlogic.NugetDependabot.Entities;

public class ActionInputs
{
    private string _nugetSource;
    private string _allowed;
    
    [Option('n', "nuget-source",
        Required = false,
        HelpText = "The Nuget Source To Override, for example: \"https://api.nuget.org/v3/octorat/index.json\".")]
    public string NugetSource
    {
        get => _nugetSource;
        set => ParseAndAssign(value, str => _nugetSource = str);
    }
    [Option('a', "allowed",
        Required = false,
        HelpText = "The Nuget Packages Allowed, for example: \"microsoft*|azure*|aws-sdk*\".")]
    public string Allowed
    {
        get => _allowed;
        set => ParseAndAssign(value, str => _allowed = str);
    }
    
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