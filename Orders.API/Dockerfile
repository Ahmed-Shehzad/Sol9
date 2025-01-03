# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Add metadata for the container image
LABEL org.opencontainers.image.title="Sol9"
LABEL org.opencontainers.image.description="This is the Docker image for the Sol9 project."
#LABEL org.opencontainers.image.source="https://github.com/ahmed-shehzad/sol9"
LABEL org.opencontainers.image.licenses="MIT"

USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Orders.API/Orders.API.csproj", "Orders.API/"]
COPY ["Orders.Application/Orders.Application.csproj", "Orders.Application/"]
COPY ["BuildingBlocks.ServiceCollection.Extensions/BuildingBlocks.ServiceCollection.Extensions.csproj", "BuildingBlocks.ServiceCollection.Extensions/"]
COPY ["BuildingBlocks.Common/BuildingBlocks.Common.csproj", "BuildingBlocks.Common/"]
COPY ["BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj", "BuildingBlocks.Contracts/"]
COPY ["BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "BuildingBlocks.Domain/"]
COPY ["BuildingBlocks.Utilities/BuildingBlocks.Utilities.csproj", "BuildingBlocks.Utilities/"]
COPY ["BuildingBlocks.Extensions/BuildingBlocks.Extensions.csproj", "BuildingBlocks.Extensions/"]
COPY ["BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "BuildingBlocks.Infrastructure/"]
COPY ["Orders.Infrastructure/Orders.Infrastructure.csproj", "Orders.Infrastructure/"]
COPY ["Orders.Domain/Orders.Domain.csproj", "Orders.Domain/"]
RUN dotnet restore "./Orders.API/Orders.API.csproj"
COPY . .
WORKDIR "/src/Orders.API"
RUN dotnet build "./Orders.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Orders.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orders.API.dll"]