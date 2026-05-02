FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TravelAgencyApi.sln ./
COPY src/TravelAgency.Api/TravelAgency.Api.csproj src/TravelAgency.Api/
COPY src/TravelAgency.Application/TravelAgency.Application.csproj src/TravelAgency.Application/
COPY src/TravelAgency.Infrastructure/TravelAgency.Infrastructure.csproj src/TravelAgency.Infrastructure/
COPY src/TravelAgency.Domain/TravelAgency.Domain.csproj src/TravelAgency.Domain/
COPY tests/TravelAgency.Tests/TravelAgency.Tests.csproj tests/TravelAgency.Tests/
RUN dotnet restore TravelAgencyApi.sln

COPY . .
RUN dotnet publish src/TravelAgency.Api/TravelAgency.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TravelAgency.Api.dll"]
