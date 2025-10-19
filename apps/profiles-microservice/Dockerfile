# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# COPY *.csproj files (relativ til Bizcord\apps\profiles-microservices)
COPY ./src/ProfileService.Api/ProfileService.Api.csproj ./src/ProfileService.Api/
COPY ./src/ProfileService.Application/ProfileService.Application.csproj ./src/ProfileService.Application/
COPY ./src/ProfileService.Contracts/ProfileService.Contracts.csproj ./src/ProfileService.Contracts/
COPY ./src/ProfileService.Domain/ProfileService.Domain.csproj ./src/ProfileService.Domain/
COPY ./src/ProfileService.Infrastructure/ProfileService.Infrastructure.csproj ./src/ProfileService.Infrastructure/

# Restore
RUN dotnet restore ./src/ProfileService.Api/ProfileService.Api.csproj

# Kopier the rest of the source
COPY . .

# Build
WORKDIR /src/src/ProfileService.Api
RUN dotnet build ProfileService.Api.csproj -c $BUILD_CONFIGURATION -o /app/build

# ---- Publish ----
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/src/ProfileService.Api
RUN dotnet publish ProfileService.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProfileService.Api.dll"]