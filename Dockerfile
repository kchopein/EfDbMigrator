#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG CONNECTION_STRING

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["EfDbMigrator.csproj", ""]
RUN dotnet restore "./EfDbMigrator.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "EfDbMigrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EfDbMigrator.csproj" -c Release --runtime linux-musl-x64 -o /app/publish -p:PublishTrimmed=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .