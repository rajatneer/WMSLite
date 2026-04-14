# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY WMSLite.Api.csproj ./
RUN dotnet restore WMSLite.Api.csproj

COPY . ./
RUN dotnet publish WMSLite.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./
RUN mkdir -p /app/Data

ENTRYPOINT ["dotnet", "WMSLite.Api.dll"]
