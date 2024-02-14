using System.Collections;
using System.Text;
using Bornlogic.NugetDependabot.Entities.Enums;

namespace Bornlogic.NugetDependabot.Entities.Configuration;

public class NugetOptions
{
    private  string _nugetSource;
    private  string _basicToken;
    private  IEnumerable<string> _allowedPackages;
    private  NugetVersionUpdateType _updateType;

    internal string GetBasicAuth() => _basicToken;
    internal string GetNugetSource() => _nugetSource;
    internal IEnumerable<string>  GetAllowedPackages() => _allowedPackages;
    internal NugetVersionUpdateType  GetAllowedUpdateType() => _updateType;
    
    internal void ConfigureOptions(string nugetSource , string username , string password , string allowedPackages, string updateType)
    {
        var nugetValidUpdateTypes = Enum.GetValuesAsUnderlyingType<NugetVersionUpdateType>().Cast<NugetVersionUpdateType>();
        
        _nugetSource = nugetSource?.EndsWith(Constants.DefaultUrlSlash) ?? false ? nugetSource.Replace(Constants.DefaultIndexJsonValue + Constants.DefaultUrlSlash, string.Empty)  : nugetSource?.Replace(Constants.DefaultIndexJsonValue, string.Empty) ?? Constants.DefaultNugetSource;
        _allowedPackages = allowedPackages?.Split(Constants.DefaultAllowedSeparator)?.ToList() ?? new List<string>();
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            _basicToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
        _updateType = nugetValidUpdateTypes.FirstOrDefault(x => updateType.Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));
    }
    
    
}