#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 443
COPY Certificates/ConfigurationServer_Client.crt /usr/local/share/ca-certificates
RUN update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ClientRestServer/ClientRestServer.csproj", "ClientRestServer/"]
RUN dotnet restore "ClientRestServer/ClientRestServer.csproj"
COPY . .
WORKDIR "/src/ClientRestServer"
RUN dotnet build "ClientRestServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClientRestServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClientRestServer.dll"]