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

        // parser.WithNotParsed(
        //     errors =>
        //     {
        //         foreach (var error in errors)
        //         {
        //             // todo enhance this
        //             Console.WriteLine($"error: {error}");
        //         }
        //         
        //         Environment.Exit(2);
        //     });

        services.Configure<NugetOptions>(options =>
        {
            options.ConfigureOptions(parser?.Value?.NugetSource, Environment.GetEnvironmentVariable("USERNAME"), Environment.GetEnvironmentVariable("PASSWORD"));
        });

        services.AddSingleton<NugetPackageService>();

        return services;
    }
}