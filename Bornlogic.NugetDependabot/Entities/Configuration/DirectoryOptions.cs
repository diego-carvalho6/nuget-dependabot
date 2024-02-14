namespace Bornlogic.NugetDependabot.Entities.Configuration;

public class DirectoryOptions
{
    private  string _workdir;
    private  string _logFileName;
    internal string GetRepositoryWorkDirectory() => _workdir;
    internal string GetLogFileName() => _logFileName;

    internal void ConfigureOptions(string workdir, string logFileName)
    {
        _workdir = workdir ?? Constants.DefaultDirectory;
        _logFileName = logFileName ?? Constants.DefaultLogFileName;
    }
}