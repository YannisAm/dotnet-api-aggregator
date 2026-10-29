# ---------- Build stage: full SDK, used only to compile ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file first and restore packages.
# This layer is cached and re-runs only when dependencies change.
COPY ApiAggregator.API/ApiAggregator.API.csproj ApiAggregator.API/
RUN dotnet restore ApiAggregator.API/ApiAggregator.API.csproj

# Copy the rest of the source and publish a Release build
COPY ApiAggregator.API/ ApiAggregator.API/
RUN dotnet publish ApiAggregator.API/ApiAggregator.API.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime stage: small image with only the ASP.NET runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Run as the built-in non-root user
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ApiAggregator.API.dll"]