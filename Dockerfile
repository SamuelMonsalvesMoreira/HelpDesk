FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/HelpDesk.Api/HelpDesk.Api.csproj", "src/HelpDesk.Api/"]
RUN dotnet restore "src/HelpDesk.Api/HelpDesk.Api.csproj"

COPY . .
WORKDIR "/src/src/HelpDesk.Api"
RUN dotnet publish "HelpDesk.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "HelpDesk.Api.dll"]

