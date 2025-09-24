# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# SDK image (dùng .NET 9 để build cả net8 và net9)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy từng csproj để restore nhanh
COPY ["LOKE/LOKE.csproj", "LOKE/"]
COPY ["Core/Core.csproj", "Core/"]

# Restore dependencies
RUN dotnet restore "LOKE/LOKE.csproj"

# Copy toàn bộ source
COPY . .

# Build
WORKDIR "/src/LOKE"
RUN dotnet build "LOKE.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "LOKE.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LOKE.dll"]
