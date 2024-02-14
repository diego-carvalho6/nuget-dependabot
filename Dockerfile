FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
COPY . ./
RUN dotnet publish "Bornlogic.NugetDependabot/Bornlogic.NugetDependabot.csproj" -c Release -o out --no-self-contained

FROM mcr.microsoft.com/dotnet/sdk:8.0
COPY --from=publish /out .
ENTRYPOINT ["dotnet", "Bornlogic.NugetDependabot.dll"]
