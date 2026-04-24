# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY CapstoneProjectTopicApproval.sln ./
COPY CapstoneRegistration.API/CapstoneRegistration.API.csproj CapstoneRegistration.API/

RUN dotnet restore CapstoneRegistration.API/CapstoneRegistration.API.csproj

# Copy everything and publish
COPY . .
RUN dotnet publish CapstoneRegistration.API/CapstoneRegistration.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Railway injects $PORT at runtime; ASP.NET Core reads ASPNETCORE_URLS
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

EXPOSE 8080

ENTRYPOINT ["dotnet", "CapstoneRegistration.API.dll"]
