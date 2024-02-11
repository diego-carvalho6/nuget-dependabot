using CommandLine;

namespace Bornlogic.NugetDependabot.Entities;

public class ActionInputs
{
    private string _password;
    private string _username;
    private string _nugetSource;
    
    [Option('u', "username",
        Required = false,
        HelpText = "The Nuget Username, for example: \"samples\".")]
    public string Username
    {
        get => _username;
        set => ParseAndAssign(value, str => _username = str);
    }
    
    [Option('n', "nuget-source",
        Required = false,
        HelpText = "The Nuget Source To Override, for example: \"https://api.nuget.org/v3/octorat/index.json\".")]
    public string NugetSource
    {
        get => _nugetSource;
        set => ParseAndAssign(value, str => _nugetSource = str);
    }
    [Option('p', "password",
        Required = false,
        HelpText = "The Nuget Password, for example: \"GHP_12345678\".")]
    public string Password
    {
        get => _password;
        set => ParseAndAssign(value, str => _password = str);
    }
    
    static void ParseAndAssign(string? value, Action<string> assign)
    {
        if (value is { Length: > 0 } && assign is not null)
        {
            assign(value.Split("/")[^1]);
        }
    }
}