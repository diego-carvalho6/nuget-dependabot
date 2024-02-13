FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bornlogic.NugetDependabot/Bornlogic.NugetDependabot.csproj", "Bornlogic.NugetDependabot/"]
RUN dotnet restore "Bornlogic.NugetDependabot/Bornlogic.NugetDependabot.csproj"
COPY . .
WORKDIR "/src/Bornlogic.NugetDependabot"
RUN dotnet build "Bornlogic.NugetDependabot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Bornlogic.NugetDependabot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bornlogic.NugetDependabot.dll"]
