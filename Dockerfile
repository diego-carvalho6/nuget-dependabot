FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bornlogic.NugetDependabot/Bornlogic.NugetDependabot.csproj", "Bornlogic.NugetDependabot/"]
RUN dotnet restore "Bornlogic.NugetDependabot/Bornlogic.NugetDependabot.csproj"
COPY . .
WORKDIR "/src/Bornlogic.NugetDependabot"
RUN dotnet build "Bornlogic.NugetDependabot.csproj" -c $BUILD_CONFIGURATION -o out --no-self-contained


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Bornlogic.NugetDependabot.csproj" -c $BUILD_CONFIGURATION -o out /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/out .
ENTRYPOINT ["dotnet", "Bornlogic.NugetDependabot.dll"]
