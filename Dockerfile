FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Shared runtime configuration for all Linux distros used by .NET images.
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

# OS/version-aware setup while preserving equivalent configuration.
RUN set -eux; \
    . /etc/os-release; \
    echo "Base OS: ${ID} ${VERSION_ID}"; \
    if [ "${ID}" = "alpine" ]; then \
        apk add --no-cache ca-certificates tzdata; \
    elif [ "${ID}" = "debian" ] || [ "${ID}" = "ubuntu" ]; then \
        apt-get update; \
        apt-get install -y --no-install-recommends ca-certificates tzdata; \
        rm -rf /var/lib/apt/lists/*; \
    else \
        echo "Unsupported Linux distribution: ${ID} ${VERSION_ID}"; \
        exit 1; \
    fi

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# OS/version-aware setup for the build stage too.
RUN set -eux; \
    . /etc/os-release; \
    echo "Build OS: ${ID} ${VERSION_ID}"; \
    if [ "${ID}" = "alpine" ]; then \
        apk add --no-cache ca-certificates tzdata; \
    elif [ "${ID}" = "debian" ] || [ "${ID}" = "ubuntu" ]; then \
        apt-get update; \
        apt-get install -y --no-install-recommends ca-certificates tzdata; \
        rm -rf /var/lib/apt/lists/*; \
    else \
        echo "Unsupported Linux distribution: ${ID} ${VERSION_ID}"; \
        exit 1; \
    fi

COPY . .
RUN dotnet restore "./Lumen.API/Lumen.API.csproj"
RUN dotnet publish "./Lumen.API/Lumen.API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Lumen.API.dll"]