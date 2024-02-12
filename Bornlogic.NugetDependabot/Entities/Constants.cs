namespace Bornlogic.NugetDependabot.Entities;

public static class Constants
{
    public const string IncludeIdentifier = "include";
    public const string VersionIdentifier = "version";
    public const string DefaultSpecsSeparator = " ";
    public const string DefaultSpecsDataSeparator = "=";
    public const string DefaultNugetSource = "https://api.nuget.org/v3/";
    public const string DefaultNugetRegistry = "registration5-gz-semver2";
    public const string DefaultUrlSlash = "/";
    public const string DefaultHiddenDirectoryPrefix = ".";
    public const string DefaultDirectory = "/home/diego/RiderProjects/ActionDependencies";
    public const string DefaultIndexJsonValue = "index.json";
    public const string BasicHeader = "Basic ";
    public const string AuthorizationHeader = "Authorization";
    public const string NugetValidType = "catalog:CatalogPage";
    public const string DefaultVersionSeparator = ".";
}