using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
    
    private readonly Regex _packageReferenceRegex = new ("(<packagereference).+?(\\/>)", RegexOptions.IgnoreCase);
    private readonly Regex _invalidValuesToRemove = new Regex("(\\\u0022)");
    private static readonly HttpClient Client = new HttpClient( new HttpClientHandler()
    {
        AutomaticDecompression = DecompressionMethods.All
    });

    public NugetPackageService(IOptions<NugetOptions> nugetOptions, ILogger<NugetPackageService> logger)
    {
        _logger = logger;
        _nugetOptions = nugetOptions.Value;
        _packages = new List<Package>();
    }
    
    public async Task<string> UpdatePackagesInFile(string fileContent, string fileName)
    {
        var references = new List<string>();
        var packages = new List<Package>();
        
        var itemGroups = fileContent.Split("\n");
        foreach (var itemGroup in itemGroups)
            references.AddRange(_packageReferenceRegex.Matches(itemGroup?.ToString() ?? string.Empty).Select(x => x.ToString()));

        foreach (var reference in references.Chunk(20))
            packages.AddRange((await Task.WhenAll(reference.Select(GetPackageSpecs))).ToList());
        
        var sbContent = new StringBuilder(fileContent);

        foreach (var package in packages.Where(x => x.HasUpdate()))
        foreach (var packageReference in package.GetSavedPackageReferences())
            sbContent.Replace(packageReference, package.ToNugetPackageReference());
        
        _logger.LogInformation(packages.Any(x => x.HasUpdate())
            ? $"Conclude update in file {fileName} \nUpdated Packages: /n {string.Join("/n", packages.Select(x => x.GetPackageName()))}"
            : $"Conclude update in file {fileName} \nNo Packages Updated \n");
        
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
            return new Package();
        
        var parsedVersion = _invalidValuesToRemove.Replace(versionValue, string.Empty);
        var parsedInclude = _invalidValuesToRemove.Replace(includeValue, string.Empty);

        if (!IsAllowedPackage(includeValue))
            return new Package();
        
        var semaphore = new SemaphoreSlim(1, 1);

        await semaphore.WaitAsync();
        
        var savedPackage = _packages.FirstOrDefault(x => x.GetPackageName() == parsedInclude);

        if (savedPackage != null)
        {
            savedPackage.UpdatePackageReferences(value);
            return savedPackage;
        }
    
        var nugetPackageVersion = await TryFindNugetPackageLastVersion(parsedInclude);

        var newPackage = new Package(value, parsedInclude, parsedVersion, nugetPackageVersion);
    
        _packages.Add(newPackage);

        semaphore.Release();
    
        return newPackage;
    }

    private bool IsAllowedPackage(string packageName)
    {
        if (!_nugetOptions.GetAllowedPackages().Any())
            return true;

        var result = false;

        return _nugetOptions.GetAllowedPackages().Any(x =>
            x.EndsWith(Constants.DefaultWildCardValue)
                ? packageName.StartsWith(x.Replace(Constants.DefaultWildCardValue, string.Empty),
                    StringComparison.InvariantCultureIgnoreCase)
                : x.Equals(packageName, StringComparison.InvariantCultureIgnoreCase));
    }

    private async Task<string> TryFindNugetPackageLastVersion(string packageName)
    {
        try
        {
            var isDefaultSource = _nugetOptions.GetNugetSource().Equals(Constants.DefaultNugetSource);
            
            return isDefaultSource ? await GetLastReferenceInPublicNugetSource(packageName) :await GetLastReferenceInPrivateNugetSource(packageName);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"\n Get Failure. \n Error Message :{e.Message} \n\n ");
        }
        
        return string.Empty;
    }

    private async Task<string> GetLastReferenceInPrivateNugetSource(string packageName)
    {
        var currentSourceUrl = $"{_nugetOptions.GetNugetSource().Clone()}{packageName}{Constants.DefaultUrlSlash}{Constants.DefaultIndexJsonValue}".ToLower();
            
        _logger.LogInformation($"GET Reference: {currentSourceUrl} In Private Source.\n");

        return await FindLastReferenceInNugetSource(currentSourceUrl,  _nugetOptions.GetBasicAuth(),
            "Not Found Package Reference In Private Source Trying To Search In Public Source.\n",
            () => GetLastReferenceInPublicNugetSource(packageName));
    }
    private async Task<string> GetLastReferenceInPublicNugetSource(string packageName)
    {
        var currentSourceUrl = $"{Constants.DefaultNugetSource}{Constants.DefaultNugetRegistry}{Constants.DefaultUrlSlash}{packageName}{Constants.DefaultUrlSlash}{Constants.DefaultIndexJsonValue}".ToLower();
            
        _logger.LogInformation($"GET Reference: {currentSourceUrl} In Public Source\n");

        return await FindLastReferenceInNugetSource(currentSourceUrl);
    }

    private async Task<string> FindLastReferenceInNugetSource(string sourceUrl, string authorization = null, string notFoundFallbackMessage = null, Func<Task<string>> notFoundAction = null)
    {
        using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, sourceUrl);
            
        if (!string.IsNullOrWhiteSpace(authorization))
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.BasicHeader, authorization);

        using HttpResponseMessage response = await Client.SendAsync(requestMessage);
        
        if (response?.StatusCode == HttpStatusCode.NotFound && notFoundAction != null)
        {
            _logger.LogError($"{notFoundFallbackMessage}");
            return await notFoundAction.Invoke();
        }
        
        response.EnsureSuccessStatusCode();
            
        var responseBody =  JsonConvert.DeserializeObject<NugetListResponse>((await response.Content.ReadAsStringAsync()) ?? string.Empty);

        var lastReference = NugetParser.ReturnGreater(responseBody?.Items?.Select(x => x?.Upper));
            
        if (!string.IsNullOrWhiteSpace(lastReference))
            _logger.LogInformation($"GET Success. {sourceUrl} Last Reference: {lastReference} \n\n");
        else
            _logger.LogInformation($"GET Success. {sourceUrl} Not Found Last Reference \n\n");

        return lastReference;
    }
}