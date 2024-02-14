using Bornlogic.NugetDependabot.Entities;
using Bornlogic.NugetDependabot.Entities.Configuration;
using Microsoft.Extensions.Logging;

namespace Bornlogic.NugetDependabot.Services;

public class DirectoryService
{
    private readonly DirectoryOptions _directoryOptions;
    private readonly NugetPackageService _nugetPackageService;
    private readonly ILogger _logger;

    public DirectoryService(DirectoryOptions directoryOptions, NugetPackageService nugetPackageService, ILogger logger)
    {
        _directoryOptions = directoryOptions;
        _nugetPackageService = nugetPackageService;
        _logger = logger;
    }

    public async Task FindDirectoriesAndUpdatePackages() => await FindDirectoriesAndUpdatePackages(_directoryOptions.GetRepositoryWorkDirectory());
    
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
}