# Auditor — build context is the solution root (where the Auditor/ folder is).
#
#   Web app:     docker build -t sirwagner44/xauditor:latest .
#   Migrations:  docker build --target migrations -t sirwagner44/xauditor:latest-migrations .
#
# The last stage (final) is the default, so a plain "docker build" produces the web app.
# Auditor/Dockerfile is a copy of this file for Visual Studio's container tooling.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# ---------- build / publish ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Auditor/Auditor.csproj", "Auditor/"]
RUN dotnet restore "Auditor/Auditor.csproj"
COPY . .
WORKDIR "/src/Auditor"
RUN dotnet publish "Auditor.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# ---------- EF Core migrations bundle ----------
FROM build AS bundle
RUN dotnet tool install --global dotnet-ef --version 8.0.23
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef migrations bundle --configuration Release --output /app/efbundle --force

# ---------- migrations image (one-shot) ----------
# Applies pending migrations only (creates the database if needed); a no-op when it's up to date.
# Needs ConnectionStrings__AudConnection in the environment.
FROM base AS migrations
WORKDIR /migrations
COPY --from=bundle /app/efbundle .
COPY --from=build /src/Auditor/appsettings.json .
RUN mkdir -p Logs && chown app Logs
USER app
ENTRYPOINT ["./efbundle"]

# ---------- web app (last stage = default) ----------
FROM base AS final
COPY --from=build /app/publish .
# Logs: Serilog rolling files. data/: sign-in keys and audit attachments (mount a volume here).
RUN mkdir -p Logs data/keys data/uploads && chown -R app Logs data
ENV DataProtection__KeysPath=/app/data/keys \
    Storage__UploadsPath=/app/data/uploads
VOLUME ["/app/data"]
USER app
ENTRYPOINT ["dotnet", "Auditor.dll"]
