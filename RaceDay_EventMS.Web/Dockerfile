# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RaceDay_EventMS.Web.csproj", "./"]
RUN dotnet restore "RaceDay_EventMS.Web.csproj"
COPY . .
RUN dotnet publish "RaceDay_EventMS.Web.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RaceDay_EventMS.Web.dll"]