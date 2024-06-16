FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
LABEL maintainer="Kamil Janek"
WORKDIR /source

COPY . .

RUN dotnet restore
RUN dotnet publish API/src/TibiaStalker.Api/TibiaStalker.Api.csproj -c Release -o /app --no-restore
RUN dotnet publish Seeders/src/WorldScanAnalyser/WorldScanAnalyser.csproj -c Release -o /app --no-restore
RUN dotnet publish Seeders/src/WorldScanAnalyserSubscriber/WorldScanAnalyserSubscriber.csproj -c Release -o /app --no-restore
RUN dotnet publish Seeders/src/DbTableCleaner/DbCleaner.csproj -c Release -o /app --no-restore
RUN dotnet publish Seeders/src/WorldScanSeeder/WorldScanSeeder.csproj -c Release -o /app --no-restore
RUN dotnet publish Seeders/src/WorldSeeder/WorldSeeder.csproj -c Release -o /app --no-restore
RUN dotnet publish Seeders/src/ChangeNameDetector/ChangeNameDetector.csproj -c Release -o /app --no-restore
RUN dotnet publish Seeders/src/ChangeNameDetectorSubscriber/ChangeNameDetectorSubscriber.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

COPY --from=build /app ./

EXPOSE 80