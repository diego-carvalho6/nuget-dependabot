FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
COPY . ./
RUN dotnet publish "Bornlogic.NugetDependabot.csproj" -c $BUILD_CONFIGURATION -o out --no-self-contained

FROM mcr.microsoft.com/dotnet/sdk:8.0
COPY --from=publish /out .
ENTRYPOINT ["dotnet", "Bornlogic.NugetDependabot.dll"]
