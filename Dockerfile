# syntax=docker/dockerfile:1

# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ENV DOTNET_ROLL_FORWARD=LatestMajor
WORKDIR /src

# Restore first (cached layer) — copy only the project files.
COPY MatchFixer-Web-App.sln ./
COPY MatchFixer-Web-App/MatchFixer-Web-App.csproj                 MatchFixer-Web-App/
COPY MatchFixer.Common/MatchFixer.Common.csproj                  MatchFixer.Common/
COPY MatchFixer.Core/MatchFixer.Core.csproj                      MatchFixer.Core/
COPY MatchFixer.Infrastructure/MatchFixer.Infrastructure.csproj  MatchFixer.Infrastructure/
RUN dotnet restore MatchFixer-Web-App/MatchFixer-Web-App.csproj

# Copy the rest of the source and publish.
COPY . .
RUN dotnet publish MatchFixer-Web-App/MatchFixer-Web-App.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

# Kestrel listens on 8080 as the non-root 'app' user.
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DP_KEYS_PATH=/app/keys

EXPOSE 8080
VOLUME /app/keys
# Run as the image's non-root 'app' user (uid 1654). The bind-mounted host
# folders (keys, team-logos) MUST be writable by uid 1654 - see deploy notes.
USER $APP_UID
ENTRYPOINT ["dotnet", "MatchFixer-Web-App.dll"]
