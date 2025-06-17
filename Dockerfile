# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5004

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["ProgressTrackingSystem.Api.csproj", "."]
RUN dotnet restore "ProgressTrackingSystem.Api.csproj"

# Copy remaining source and publish
COPY . .
RUN dotnet publish "ProgressTrackingSystem.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS progress-tracking
WORKDIR /app
COPY --from=build /app/publish .

# Environment settings
ENV ASPNETCORE_URLS="http://0.0.0.0:5004"
ENV ASPNETCORE_ENVIRONMENT="Production"

# Install dependencies
RUN apt-get update && apt-get install -y libkrb5-3

# Entry point
ENTRYPOINT ["dotnet", "ProgressTrackingSystem.Api.dll"]
