using CommandLine;

namespace Bornlogic.NugetDependabot.Entities;

public class ActionInputs
{
    private string _nugetSource;
    
    public string NugetSource
    {
        get => _nugetSource;
        set => ParseAndAssign(value, str => _nugetSource = str);
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