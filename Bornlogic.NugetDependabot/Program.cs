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

        await Get<DirectoryService>(host)
            ?.FindDirectoriesAndUpdatePackages();
        
        await host.RunAsync();
    }

    static TService Get<TService>(IHost host)
        where TService : notnull =>
        host.Services.GetRequiredService<TService>();


}



