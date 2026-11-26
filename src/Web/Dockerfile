FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.sln ./
COPY Directory.Build.props ./
COPY src/Contracts/*.csproj src/Contracts/
COPY src/Web/*.csproj src/Web/
RUN dotnet restore src/Web/Web.csproj

COPY . .
RUN dotnet publish src/Web/Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Web.dll"]