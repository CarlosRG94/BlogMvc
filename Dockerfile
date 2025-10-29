# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app
COPY . .

RUN dotnet restore "BlogMVC/BlogMVC.csproj"
RUN dotnet build "BlogMVC/BlogMVC.csproj" -c Release
RUN dotnet publish "BlogMVC/BlogMVC.csproj" -c Release -o out

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Configurar puerto para Railway
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENTRYPOINT ["dotnet", "BlogMVC.dll"]
