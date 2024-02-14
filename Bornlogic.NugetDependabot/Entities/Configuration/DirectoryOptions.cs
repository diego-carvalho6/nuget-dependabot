namespace Bornlogic.NugetDependabot.Entities.Configuration;

public class DirectoryOptions
{
    private  string _workdir;
    internal string GetRepositoryWorkDirectory() => _workdir;

    internal void ConfigureOptions(string workdir)
    {
        _workdir = workdir ?? Constants.DefaultDirectory;
    }
}