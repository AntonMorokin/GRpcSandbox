#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 443
COPY Certificates/ConfigurationServer_Server.pfx /root/.aspnet/ssl-certs/

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ConfigurationServer/ConfigurationServer.csproj", "ConfigurationServer/"]
RUN dotnet restore "ConfigurationServer/ConfigurationServer.csproj"
COPY . .
WORKDIR "/src/ConfigurationServer"
RUN dotnet build "ConfigurationServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConfigurationServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConfigurationServer.dll"]