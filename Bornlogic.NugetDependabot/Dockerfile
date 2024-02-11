FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

COPY . ./
RUN dotnet publish ./Bornlogic.NugetDependabot/Bornlogic.NugetDependabot.csproj -c Release -o out --no-self-contained

LABEL com.github.actions.name="Bornlogic Nuget Dependabot"
LABEL com.github.actions.description="A Github action deal with updates in csproj nuget package dependencies."
LABEL com.github.actions.icon="user-plus"
LABEL com.github.actions.color="gray-dark"

FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app
COPY --from=base /out .
ENTRYPOINT ["dotnet", "Bornlogic.NugetDependabot.dll"]
