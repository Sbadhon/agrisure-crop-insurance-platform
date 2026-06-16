FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/Contracts/AgriSure.Contracts/AgriSure.Contracts.csproj src/Contracts/AgriSure.Contracts/
COPY src/BuildingBlocks/AgriSure.BuildingBlocks/AgriSure.BuildingBlocks.csproj src/BuildingBlocks/AgriSure.BuildingBlocks/
COPY src/Gateway/AgriSure.Gateway/AgriSure.Gateway.csproj src/Gateway/AgriSure.Gateway/
RUN dotnet restore src/Gateway/AgriSure.Gateway/AgriSure.Gateway.csproj
COPY . .
RUN dotnet publish src/Gateway/AgriSure.Gateway/AgriSure.Gateway.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AgriSure.Gateway.dll"]
