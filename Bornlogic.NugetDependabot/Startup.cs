using Bornlogic.NugetDependabot.Entities;
using Bornlogic.NugetDependabot.Entities.Configuration;
using Bornlogic.NugetDependabot.Services;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using static CommandLine.Parser;

namespace Bornlogic.NugetDependabot;

public static class Startup
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, string[] args)
    {
        ParserResult<ActionInputs> parser = Default.ParseArguments<ActionInputs>(() => new(), args);
        
        services.AddLogging();
        
        services.Configure<NugetOptions>(options =>
        {
            options.ConfigureOptions(parser?.Value?.NugetSource, Environment.GetEnvironmentVariable("USERNAME"), Environment.GetEnvironmentVariable("PASSWORD"), parser?.Value?.AllowedSources);
        });
        services.Configure<DirectoryOptions>(options =>
        {
            options.ConfigureOptions(parser?.Value?.WorkspaceDirectory);
        });

        services.AddSingleton<NugetPackageService>();
        services.AddSingleton<DirectoryService>();

        return services;
    }
}