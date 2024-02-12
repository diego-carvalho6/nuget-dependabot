using System.Text;
using System.Text.RegularExpressions;
using Bornlogic.NugetDependabot.Entities;
using Bornlogic.NugetDependabot.Entities.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bornlogic.NugetDependabot.Services;

public class NugetPackageService
{
    private readonly List<Package> _packages;
    private readonly NugetOptions _nugetOptions;
    private readonly ILogger<NugetPackageService> _logger;
    
    private readonly Regex _itemGroupRegex = new ("(?s)(?<=<itemgroup>).*?(?=<\\/itemgroup)>");
    private readonly Regex _packageReferenceRegex = new ("(<packagereference).+?(\\/>)");
    private readonly Regex _invalidValuesToRemove = new Regex("(\\\u0022)");
    private static readonly HttpClient Client = new HttpClient();

    public NugetPackageService(IOptions<NugetOptions> nugetOptions, ILogger<NugetPackageService> logger)
    {
        _logger = logger;
        _nugetOptions = nugetOptions.Value;
        _packages = new List<Package>();
    }
    
    public async Task<string> UpdatePackagesInFile(string fileContent)
    {
        var references = new List<string>();
        var packages = new List<Package>();
        
        var itemGroups = _itemGroupRegex.Matches(fileContent);
        foreach (var itemGroup in itemGroups)
            references.AddRange(_packageReferenceRegex.Matches(itemGroup?.ToString() ?? string.Empty).Select(x => x.ToString()));

        foreach (var reference in references.Chunk(20))
            packages.AddRange((await Task.WhenAll(reference.Select(GetPackageSpecs))).ToList());
        
        var sbContent = new StringBuilder(fileContent);

        foreach (var package in packages)
        foreach (var packageReference in package.GetSavedPackageReferences())
            sbContent.Replace(packageReference, package.ToNugetPackageReference());
        
        return sbContent.ToString();
    }
    
    private async Task<Package> GetPackageSpecs(string value)
    {
        var splitedValue = value.Split(Constants.DefaultSpecsSeparator);

        var includeValue = splitedValue.FirstOrDefault(x => x.ToLowerInvariant().StartsWith(Constants.IncludeIdentifier))
            ?.Split(Constants.DefaultSpecsDataSeparator)?.LastOrDefault();    
        
        var versionValue = splitedValue.FirstOrDefault(x => x.ToLowerInvariant().StartsWith(Constants.VersionIdentifier))
            ?.Split(Constants.DefaultSpecsDataSeparator)?.LastOrDefault();

        if (string.IsNullOrWhiteSpace(includeValue) || string.IsNullOrWhiteSpace(versionValue))
            return default;
        
        var parsedVersion = _invalidValuesToRemove.Replace(versionValue, string.Empty);
        var parsedInclude = _invalidValuesToRemove.Replace(includeValue, string.Empty);

        var semaphore = new SemaphoreSlim(1, 1);

        await semaphore.WaitAsync();
        
        var savedPackage = _packages.FirstOrDefault(x => x.GetPackageName() == parsedInclude);

        if (savedPackage != null)
        {
            savedPackage.UpdatePackageReferences(value);
            return savedPackage;
        }
    
        var nugetPackageVersion = await TryFindNugetPackageVersion(parsedInclude);

        var newPackage = new Package(value, parsedInclude, parsedVersion, nugetPackageVersion);
    
        _packages.Add(newPackage);

        semaphore.Release();
    
        return newPackage;
    }

    private async Task<string> TryFindNugetPackageVersion(string packageName)
    {
        try
        {
            var currentSourceUrl = $"{_nugetOptions.GetNugetSource().Clone()}{packageName}{Constants.DefaultUrlSlash}{Constants.DefaultIndexJsonValue}";
            
            _logger.LogInformation($"GET Reference: {currentSourceUrl} \n");
            
            using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, currentSourceUrl);
            
            if (!string.IsNullOrWhiteSpace(_nugetOptions.GetBasicAuth()))
                requestMessage.Headers.Add(Constants.AuthorizationHeader, Constants.BasicHeader + _nugetOptions.GetBasicAuth());
            
            using HttpResponseMessage response = await Client.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            
            string responseBody = await response.Content.ReadAsStringAsync();
            
            var nugetResponse = JsonConvert.DeserializeObject<NugetListResponse>(responseBody);
            
            var lastReference = nugetResponse?.Items?.FirstOrDefault()?.Upper;
            
            if (!string.IsNullOrWhiteSpace(lastReference))
                _logger.LogInformation($"GET Success.  Last Reference: {lastReference} \n\n");
            else
                _logger.LogInformation($"GET Success. Not Found Last Reference \n\n");
            
            return lastReference;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"\nException Caught! \n Message :{e.Message} \n\n ");
        }
        
        return string.Empty;
    }
}