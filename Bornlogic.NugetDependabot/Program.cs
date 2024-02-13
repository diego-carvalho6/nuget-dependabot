using System.Text;
using Bornlogic.NugetDependabot;
using Bornlogic.NugetDependabot.Entities;
using Bornlogic.NugetDependabot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

static class Program
{

    [STAThread]
    static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Services.ConfigureServices(args);

        using IHost host = builder.Build();

        await FindDirectoriesAndUpdatePackages(Constants.DefaultDirectory, host);
        
        await host.RunAsync();
    }

    static async Task FindDirectoriesAndUpdatePackages(string path, IHost host)
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
                await FindDirectoriesAndUpdatePackages(directory, host);
            
            var files = Directory.GetFiles(path, "*csproj", defaultOptions);

            foreach (var csProjFile in files)
                await ProcessNugetFileUpdate(csProjFile, host);
        
        }
        catch (Exception e)
        {
            var logger = Get<ILogger<NugetPackageService>>(host);
            logger.LogError($"Failed To Read Files In Directory {path}. Error: {e.Message}" );   
        }
       
    }

    static async Task ProcessNugetFileUpdate(string path, IHost host)
    {
        try
        {
            if (!File.Exists(path))
                return;

            var allContent = await File.ReadAllTextAsync(path);

            var newContent =
                await (Get<NugetPackageService>(host)
                           ?.UpdatePackagesInFile(allContent, path.Split(Constants.DefaultUrlSlash).LastOrDefault()) ??
                       Task.FromResult(allContent));
            
            await File.WriteAllTextAsync(path, newContent);
        }
        catch (Exception e)
        {
            var logger = Get<ILogger<NugetPackageService>>(host);
            logger.LogError($"Failed To Update File {path}. Error: {e.Message}" );   
        }
       
    }

    static TService Get<TService>(IHost host)
        where TService : notnull =>
        host.Services.GetRequiredService<TService>();


}



