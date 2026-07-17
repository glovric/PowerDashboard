# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY powerservice/. ./powerservice/
COPY Shared/Config.cs ./Shared/Config.cs

RUN dotnet restore ./powerservice/powerservice.csproj

WORKDIR /src/powerservice

RUN dotnet publish \
    -c Release \
    -o /app/publish \
    -p:SatelliteResourceLanguages=en

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final

WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# ASP.NET application
COPY --from=build /app/publish .
COPY Shared/sharedsettings.json /Shared/sharedsettings.json

RUN chown -R appuser:appgroup /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "powerservice.dll"]