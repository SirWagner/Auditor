FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Auditor/Auditor.csproj", "Auditor/"]
RUN dotnet restore "Auditor/Auditor.csproj"
COPY . .
WORKDIR "/src/Auditor"
RUN dotnet publish "Auditor.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Auditor.dll"]
