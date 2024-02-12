using Bornlogic.NugetDependabot;
using Bornlogic.NugetDependabot.Entities;
using Bornlogic.NugetDependabot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.ConfigureServices(args);

using IHost host = builder.Build();

await FindDirectoriesAndUpdatePackages(Constants.DefaultDirectory, host);
await host.RunAsync();

static async Task FindDirectoriesAndUpdatePackages(string path,  IHost host)
{
    var directories = Directory.GetDirectories(path);

    var publicDirectories = directories.Where(x => !x.Split(Constants.DefaultUrlSlash)?.LastOrDefault().StartsWith(Constants.DefaultHiddenDirectoryPrefix) ?? false);
    
    foreach (var directory in publicDirectories)
        await FindDirectoriesAndUpdatePackages(directory, host);
        
    var files = Directory.GetFiles(path);
    var csProjFiles = files.Where(x => x.EndsWith(".csproj"));

    foreach (var csProjFile in csProjFiles)
        await ProcessNugetFileUpdate(csProjFile, host);
}
static async Task ProcessNugetFileUpdate(string path,  IHost host)
{
    if (!File.Exists(path))
        return;
    
    var allContent = await File.ReadAllTextAsync(path);
    
    var newContent = await (Get<NugetPackageService>(host)?.UpdatePackagesInFile(allContent) ??  Task.FromResult(allContent));

    await File.WriteAllTextAsync(path, newContent);
}
   
static TService Get<TService>(IHost host)
    where TService : notnull =>
    host.Services.GetRequiredService<TService>();





