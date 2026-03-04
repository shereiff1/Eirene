# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution
COPY Eirene.sln .

# Copy project files
COPY Eirene.API/Eirene.API.csproj EireneWebAPI/
COPY EireneMVC/EireneMVC.csproj EireneMVC/
COPY Eirene.BLL/Eirene.BLL.csproj BLL/
COPY Eirene.DAL/Eirene.DAL.csproj DAL/

# Restore dependencies
RUN dotnet restore Eirene.sln

# Copy everything else
COPY . .

# Publish API
WORKDIR /src/EireneWebAPI
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EireneWebAPI.dll"]
