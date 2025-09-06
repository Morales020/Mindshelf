# Use the official .NET 8 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["MindShelf_PL/MindShelf_PL/MindShelf_PL.csproj", "MindShelf_PL/MindShelf_PL/"]
COPY ["MindShelf_PL/MindShelf_BL/MindShelf_BL.csproj", "MindShelf_PL/MindShelf_BL/"]
COPY ["MindShelf_PL/MindShelf_DAL/MindShelf_DAL.csproj", "MindShelf_PL/MindShelf_DAL/"]

# Restore dependencies
RUN dotnet restore "MindShelf_PL/MindShelf_PL/MindShelf_PL.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/MindShelf_PL/MindShelf_PL"
RUN dotnet build "MindShelf_PL.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MindShelf_PL.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for uploaded files
RUN mkdir -p /app/wwwroot/Images/Book
RUN mkdir -p /app/wwwroot/Images/Author

ENTRYPOINT ["dotnet", "MindShelf_PL.dll"]
