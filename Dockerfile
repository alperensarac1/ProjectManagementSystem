# =========================================================
# BUILD STAGE
# =========================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Önce proje dosyalarını kopyalıyoruz.
# Böylece NuGet restore katmanı kaynak kod değişmedikçe cache'te kalır.
COPY ProjectManagement.Domain/ProjectManagement.Domain.csproj \
     ProjectManagement.Domain/

COPY ProjectManagement.Application/ProjectManagement.Application.csproj \
     ProjectManagement.Application/

COPY ProjectManagement.Infrastructure/ProjectManagement.Infrastructure.csproj \
     ProjectManagement.Infrastructure/

COPY ProjectManagement.Api/ProjectManagement.Api.csproj \
     ProjectManagement.Api/

# API projesi üzerinden bağımlılıkları restore eder.
RUN dotnet restore \
    ProjectManagement.Api/ProjectManagement.Api.csproj

# Kaynak kodların tamamını kopyalar.
COPY . .

# Release çıktısını publish klasörüne üretir.
RUN dotnet publish \
    ProjectManagement.Api/ProjectManagement.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false


# =========================================================
# RUNTIME STAGE
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

USER root

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/data /app/backups \
    && chown -R app:app /app/data /app/backups

COPY --from=build /app/publish .

RUN chown -R app:app /app

USER app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "ProjectManagement.Api.dll"]