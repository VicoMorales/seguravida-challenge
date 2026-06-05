FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SeguraVida.Claims.sln ./
COPY NuGet.config ./
COPY Directory.Build.props ./
COPY src/backend/SeguraVida.Claims.Api/SeguraVida.Claims.Api.csproj src/backend/SeguraVida.Claims.Api/
COPY src/backend/SeguraVida.Claims.Application/SeguraVida.Claims.Application.csproj src/backend/SeguraVida.Claims.Application/
COPY src/backend/SeguraVida.Claims.Domain/SeguraVida.Claims.Domain.csproj src/backend/SeguraVida.Claims.Domain/
COPY src/backend/SeguraVida.Claims.Infrastructure/SeguraVida.Claims.Infrastructure.csproj src/backend/SeguraVida.Claims.Infrastructure/
RUN dotnet restore src/backend/SeguraVida.Claims.Api/SeguraVida.Claims.Api.csproj --configfile NuGet.config

COPY . .
RUN dotnet publish src/backend/SeguraVida.Claims.Api/SeguraVida.Claims.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SeguraVida.Claims.Api.dll"]
