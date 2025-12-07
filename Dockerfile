FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish ./src/CapitalGains.Cli/CapitalGains.Cli.csproj -c Release -o /app/publish --nologo

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapitalGains.Cli.dll"]


