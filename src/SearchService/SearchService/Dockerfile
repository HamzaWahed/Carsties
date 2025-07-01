FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
ARG BUILD_CONFIGURATION=Release

COPY Carsties.sln ./
COPY src/SearchService/SearchService/SearchService.csproj src/SearchService/SearchService/
COPY src/Contracts/Contracts.csproj src/Contracts/

RUN dotnet restore "src/SearchService/SearchService/SearchService.csproj"

COPY . .

RUN dotnet publish "src/SearchService/SearchService/SearchService.csproj"  -c $BUILD_CONFIGURATION -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SearchService.dll"]