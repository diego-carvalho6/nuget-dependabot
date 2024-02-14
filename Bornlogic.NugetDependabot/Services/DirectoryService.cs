using System.Text;
using Bornlogic.NugetDependabot.Entities;
using Bornlogic.NugetDependabot.Entities.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bornlogic.NugetDependabot.Services;

public class DirectoryService
{
    private readonly DirectoryOptions _directoryOptions;
    private readonly NugetPackageService _nugetPackageService;
    private readonly ILogger<DirectoryService> _logger;

    public DirectoryService(IOptions<DirectoryOptions> directoryOptions, NugetPackageService nugetPackageService, ILogger<DirectoryService> logger)
    {
        _directoryOptions = directoryOptions.Value;
        _nugetPackageService = nugetPackageService;
        _logger = logger;
    }

    public async Task FindDirectoriesAndUpdatePackages(bool updateLogFile = true)
    {
        await FindDirectoriesAndUpdatePackages(_directoryOptions.GetRepositoryWorkDirectory());
        if (updateLogFile)
            await SaveLogIfAvailable();
    }

    private  async Task FindDirectoriesAndUpdatePackages(string path)
    {
        try
        {
            var defaultOptions = new EnumerationOptions()
            {
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.Encrypted | FileAttributes.NotContentIndexed | FileAttributes.IntegrityStream | FileAttributes.ReparsePoint | FileAttributes.SparseFile | FileAttributes.Archive | FileAttributes.Compressed |
                                   FileAttributes.Offline | FileAttributes.System | FileAttributes.Temporary,
                IgnoreInaccessible = true,
            };
            
            var directories = Directory.GetDirectories(path, Constants.DefaultWildCardValue,
                enumerationOptions: defaultOptions );

            foreach (var directory in directories)
                await FindDirectoriesAndUpdatePackages(directory);
            
            var files = Directory.GetFiles(path, "*csproj", defaultOptions);

            foreach (var csProjFile in files)
                await ProcessNugetFileUpdate(csProjFile);
        
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed To Read Files In Directory {path}. Error: {e.Message}" );   
        }
       
    }

    private async Task ProcessNugetFileUpdate(string path)
    {
        try
        {
            if (!File.Exists(path))
                return;

            var allContent = await File.ReadAllTextAsync(path);

            var newContent =
                await _nugetPackageService.UpdatePackagesInFile(allContent, path.Split(Constants.DefaultUrlSlash).LastOrDefault());
            
            await File.WriteAllTextAsync(path, newContent);
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed To Update File {path}. Error: {e.Message}" );   
        }
    }
    
    private async Task SaveLogIfAvailable()
    {
        var packages = _nugetPackageService.GetUpdatedPackages();

        if (!packages.Any())
        {
            _logger.LogInformation("\nNo Packages To Update, Skipping Log File\n");
            return;
        }
        
        var gitHubOutputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
        if (!string.IsNullOrWhiteSpace(gitHubOutputFile))
        {
            var prefix = "<h1>Packages Updated:</h1> <ul>";
            var sufix = "</ul>";
            
            var detailsMessage = packages.Any() ? $"{string.Join("", packages.Select(x => $"<li> {x.GetPackageName()}: {x.GetVersionComparator()} </li>" ))}"
                : $"No Packages Updated";
            
            using StreamWriter textWriter = new(gitHubOutputFile, true, Encoding.UTF8);
            textWriter.WriteLine($"title=Updated-{packages.Count()}-Packages-At-{DateTime.UtcNow:yy-MM-dd}");
            textWriter.WriteLine($"details={prefix}{detailsMessage}{sufix}");
        }

        string path = _directoryOptions.GetLogFileName();
        var insertData = $"Update Dependencies \nCount: {packages.Count} \nAt: {DateTime.UtcNow:yy-MM-dd} \n\n";
        
        if (File.Exists(path))
        {
            string oldText = File.ReadAllText(path);
            using (var sw = new StreamWriter(path, false))
            {
                sw.Write(insertData);
                sw.Write(oldText);
            }
        }
        else File.WriteAllText(path,insertData);
    }
}