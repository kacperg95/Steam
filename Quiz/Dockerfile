FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Quiz/Quiz.csproj", "Quiz/"]
COPY ["DTO/DTO.csproj", "DTO/"]

RUN dotnet restore "Quiz/Quiz.csproj"

COPY . .

FROM build AS publish
RUN dotnet publish "Quiz/Quiz.csproj" -c Release -o /app/publish --no-restore

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Teraz "publish" jest rozpoznawalne
COPY --from=publish /app/publish/wwwroot .

CMD ["nginx", "-g", "daemon off;"]