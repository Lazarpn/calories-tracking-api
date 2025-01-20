# Use the .NET 7 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET 7 SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["CaloriesTracking.Api/CaloriesTracking.Api.csproj", "CaloriesTracking.Api/"]
RUN dotnet restore "CaloriesTracking.Api/CaloriesTracking.Api.csproj"

# Copy the rest of the application and build it
COPY . .
WORKDIR "/src/CaloriesTracking.Api"
RUN dotnet publish -c Release -o /app/publish

# Final image with the app
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CaloriesTracking.Api.dll"]
