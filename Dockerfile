# Use a base image for .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory in the container
WORKDIR /src

# Copy the project file(s) and restore dependencies
COPY ["CaloriesTracking.Api/CaloriesTracking.Api.csproj", "CaloriesTracking.Api/"]
COPY ["CaloriesTracking.Common/CaloriesTracking.Common.csproj", "CaloriesTracking.Common/"]
COPY ["CaloriesTracking.Core/CaloriesTracking.Core.csproj", "CaloriesTracking.Core/"]
COPY ["CaloriesTracking.Data/CaloriesTracking.Data.csproj", "CaloriesTracking.Data/"]
COPY ["CaloriesTracking.Entities/CaloriesTracking.Entities.csproj", "CaloriesTracking.Entities/"]

# Restore all dependencies (via `dotnet restore`)
RUN dotnet restore "CaloriesTracking.Api/CaloriesTracking.Api.csproj"
RUN dotnet restore "CaloriesTracking.Core/CaloriesTracking.Core.csproj"
RUN dotnet restore "CaloriesTracking.Data/CaloriesTracking.Data.csproj"
RUN dotnet restore "CaloriesTracking.Entities/CaloriesTracking.Entities.csproj"
RUN dotnet restore "CaloriesTracking.Common/CaloriesTracking.Common.csproj"

# Copy the rest of the application code
COPY . .

# Build the application
RUN dotnet publish "CaloriesTracking.Api/CaloriesTracking.Api.csproj" -c Release -o /app/publish

# ///////////////////////////////////////////////////////////
# Set the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 7068

# Copy the built application from the build image
COPY --from=build /app/publish .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "CaloriesTracking.Api.dll"]

