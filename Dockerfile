﻿FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

COPY ["Bornlogic.NugetDependabot/Bornlogic.NugetDependabot.csproj", "Bornlogic.NugetDependabot/"]
RUN dotnet restore "Bornlogic.NugetDependabot/Bornlogic.NugetDependabot.csproj"
COPY . .
WORKDIR "/Bornlogic.NugetDependabot"
RUN dotnet build "Bornlogic.NugetDependabot.csproj" -c $BUILD_CONFIGURATION -o github/workspace/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Bornlogic.NugetDependabot.csproj" -c $BUILD_CONFIGURATION -o github/workspace/publish --no-self-contained

FROM base AS final
WORKDIR github/workspace
COPY --from=publish github/workspace/publish .
ENTRYPOINT ["dotnet", "Bornlogic.NugetDependabot.dll"]
