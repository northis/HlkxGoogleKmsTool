# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Copy solution and project files
COPY src/ src/

# Publish the project
RUN dotnet publish src/OpenVsixSignTool/OpenVsixSignTool.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /app
# Copy published application
COPY --from=build /app/publish .

# Create entry point with the same parameters
ENTRYPOINT ["dotnet", "OpenVsixSignTool.dll"]
