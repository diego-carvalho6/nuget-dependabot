using System.Text;

namespace Bornlogic.NugetDependabot.Entities.Configuration;

public class NugetOptions
{
    private  string _nugetSource;
    private  string _username;
    private  string _password;
    private  string _basicToken;

    internal string GetBasicAuth() => _basicToken;
    internal string GetNugetSource() => _nugetSource;
    
    internal void ConfigureOptions(string nugetSource = null, string username = null, string password = null)
    {
        _username = username;
        _password = password;
        _nugetSource = nugetSource?.EndsWith(Constants.DefaultUrlSlash) ?? false ? nugetSource.Replace(Constants.DefaultIndexJsonValue + Constants.DefaultUrlSlash, string.Empty)  : nugetSource?.Replace(Constants.DefaultIndexJsonValue, string.Empty) ?? Constants.DefaultNugetSource;

        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            _basicToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

    }
    
    
}