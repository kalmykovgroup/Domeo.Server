FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/ApiGateway/Domeo.ApiGateway/Domeo.ApiGateway.csproj", "src/ApiGateway/Domeo.ApiGateway/"]
COPY ["src/Shared/Domeo.Shared.Auth/Domeo.Shared.Auth.csproj", "src/Shared/Domeo.Shared.Auth/"]
COPY ["src/Shared/Domeo.Shared.Infrastructure/Domeo.Shared.Infrastructure.csproj", "src/Shared/Domeo.Shared.Infrastructure/"]
COPY ["src/Shared/Domeo.Shared.Contracts/Domeo.Shared.Contracts.csproj", "src/Shared/Domeo.Shared.Contracts/"]
COPY ["src/Shared/Domeo.Shared.Kernel/Domeo.Shared.Kernel.csproj", "src/Shared/Domeo.Shared.Kernel/"]
RUN dotnet restore "src/ApiGateway/Domeo.ApiGateway/Domeo.ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/ApiGateway/Domeo.ApiGateway"
RUN dotnet build "Domeo.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Domeo.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Domeo.ApiGateway.dll"]
