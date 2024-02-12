using System.Text;

namespace Bornlogic.NugetDependabot.Entities.Configuration;

public class NugetOptions
{
    private  string _nugetSource;
    private  string _basicToken;
    private  IEnumerable<string> _allowedPackages;

    internal string GetBasicAuth() => _basicToken;
    internal string GetNugetSource() => _nugetSource;
    internal IEnumerable<string>  GetAllowedPackages() => _allowedPackages;
    
    internal void ConfigureOptions(string nugetSource = null, string username = null, string password = null, string allowedPackages = null)
    {
        _nugetSource = nugetSource?.EndsWith(Constants.DefaultUrlSlash) ?? false ? nugetSource.Replace(Constants.DefaultIndexJsonValue + Constants.DefaultUrlSlash, string.Empty)  : nugetSource?.Replace(Constants.DefaultIndexJsonValue, string.Empty) ?? Constants.DefaultNugetSource;
        _allowedPackages = allowedPackages?.Split(Constants.DefaultAllowedSeparator)?.ToList() ?? new List<string>();
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            _basicToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

    }
    
    
}