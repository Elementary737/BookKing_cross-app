FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.sln ./
COPY Directory.Build.props ./
COPY src/Contracts/*.csproj src/Contracts/
COPY src/Core/*.csproj src/Core/
COPY src/Infrastructure/*.csproj src/Infrastructure/
COPY src/Api/*.csproj src/Api/

RUN dotnet restore src/Api/Api.csproj

COPY . .
RUN dotnet publish src/Api/Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN mkdir -p /app/data

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]