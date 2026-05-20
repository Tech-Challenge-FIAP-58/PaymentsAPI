# syntax=docker/dockerfile:1

# =========================
# Base (runtime)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get install -y ca-certificates && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 8080

# Se não estiver usando UID customizado, mantenha comentado
# USER $APP_UID
# =========================
# Build (restore + build)
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# -------------------------
# Copia solução (se existir)
# -------------------------
# Remova esta linha se não houver .sln
COPY FCG.Payments.slnx ./

# -------------------------
# Copia apenas o csproj (cache de restore)
# -------------------------
COPY src/FCG.Payments/FCG.Payments.csproj src/FCG.Payments/

# -------------------------
# Restore
# -------------------------
RUN dotnet restore src/FCG.Payments/FCG.Payments.csproj

# -------------------------
# Copia o restante do código
# -------------------------
COPY . .

# -------------------------
# Build
# -------------------------
RUN dotnet build src/FCG.Payments/FCG.Payments.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/build


# =========================
# Publish
# =========================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish src/FCG.Payments/FCG.Payments.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


# =========================
# Final (runtime)
# =========================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCG.Payments.dll"]
