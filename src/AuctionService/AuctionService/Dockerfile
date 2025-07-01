FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
ARG BUILD_CONFIGURATION=Release

COPY Carsties.sln ./
COPY src/AuctionService/AuctionService/AuctionService.csproj src/AuctionService/AuctionService/
COPY src/Contracts/Contracts.csproj src/Contracts/

RUN dotnet restore "./src/AuctionService/AuctionService/AuctionService.csproj"

COPY . .

RUN dotnet publish "./src/AuctionService/AuctionService/AuctionService.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuctionService.dll"]
