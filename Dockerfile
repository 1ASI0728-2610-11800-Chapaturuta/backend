# 1) Build con SDK 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# (Opcional) instala clang y zlib1g-dev si haces AOT
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
    clang \
    zlib1g-dev

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# CAMBIADO: Copia el .csproj desde la raíz del contexto
COPY ["Frock-backend.csproj", "./"]
RUN dotnet restore "Frock-backend.csproj"

# Copia todo el código
COPY . .

# CAMBIADO: Ya estamos en /src, no necesitas cambiar de directorio
RUN dotnet build "Frock-backend.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# 2) Publica tu aplicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Frock-backend.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=true

# 3) Imagen de runtime con ASP.NET (incluye .NET 9)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Que Kestrel escuche en el puerto 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copia los artefactos publicados
COPY --from=publish /app/publish .

# Arranca tu aplicación
ENTRYPOINT ["dotnet", "Frock-backend.dll"]