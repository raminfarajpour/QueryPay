# syntax=docker/dockerfile:1

# 1) Base runtime image (Debian-based, not Alpine)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 5161
ENV ASPNETCORE_URLS=http://+:5161
ENV DOTNET_ENVIRONMENT=Production

# 2) Build-stage image (Debian-based SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

RUN apk add --no-cache libc6-compat

# Copy all .csproj files
COPY ["Billing.Api/Billing.Api.csproj", "Billing.Api/"]
COPY ["Billing.Application/Billing.Application.csproj", "Billing.Application/"]
COPY ["Billing.Infrastructure/Billing.Infrastructure.csproj", "Billing.Infrastructure/"]
COPY ["Billing.Domain/Billing.Domain.csproj", "Billing.Domain/"]

# Restore dependencies
RUN dotnet restore "Billing.Api/Billing.Api.csproj"

# Copy the entire solution and build
COPY . .
WORKDIR "/src/Billing.Api"
RUN dotnet build "Billing.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 3) Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Billing.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish

# 4) Final stage: minimal runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Billing.Api.dll"]