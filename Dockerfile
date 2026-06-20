FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution and project files first to cache layer
COPY ["Eirene.sln", "./"]
COPY ["Eirene.API/Eirene.API.csproj", "Eirene.API/"]
COPY ["Eirene.BLL/Eirene.BLL.csproj", "Eirene.BLL/"]
COPY ["Eirene.DAL/Eirene.DAL.csproj", "Eirene.DAL/"]
COPY ["EireneMVC/EireneMVC.csproj", "EireneMVC/"]
COPY ["Tests/Eirene.UnitTests/Eirene.UnitTests.csproj", "Tests/Eirene.UnitTests/"]
COPY ["Tests/Eirene.IntegrationTests/Eirene.IntegrationTests.csproj", "Tests/Eirene.IntegrationTests/"]
COPY ["Tests/Eirene.Tests.Shared/Eirene.Tests.Shared.csproj", "Tests/Eirene.Tests.Shared/"] 

# Restore dependencies
RUN dotnet restore "Eirene.sln"

# Copy the remaining source code
COPY . .

# Build and publish the API project
WORKDIR "/src/Eirene.API"
RUN dotnet publish "Eirene.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port (Railway injects the PORT env var dynamically)
EXPOSE 8080

ENTRYPOINT ["dotnet", "Eirene.API.dll"]
