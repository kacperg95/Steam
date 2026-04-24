FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["API/API.csproj", "API/"]
COPY ["BackgroundServices/BackgroundServices.csproj", "BackgroundServices/"]
COPY ["DTO/DTO.csproj", "DTO/"]
COPY ["QuizEngine/QuizEngine.csproj", "QuizEngine/"]
COPY ["Repository/Repository.csproj", "Repository/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["SteamApi/SteamApi.csproj", "SteamApi/"]

RUN dotnet restore "API/API.csproj"

COPY . .

RUN dotnet publish "API/API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "API.dll"]
