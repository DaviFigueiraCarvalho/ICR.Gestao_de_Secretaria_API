# Estágio de Runtime (base)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# Precisamos ser root para instalar a lib
USER root 
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
# Voltamos para o usuário seguro
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ICR.API/ICR.API.csproj", "ICR.API/"]
COPY ["ICR.Application/ICR.Application.csproj", "ICR.Application/"]
COPY ["ICR.Infrastructure/ICR.Infrastructure.csproj", "ICR.Infrastructure/"]
COPY ["ICR.Domain/ICR.Domain.csproj", "ICR.Domain/"]
RUN dotnet restore "./ICR.API/ICR.API.csproj"
COPY . .
WORKDIR "/src/ICR.API"
RUN dotnet build "./ICR.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Estágio de Publicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ICR.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Estágio Final de Produção
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ICR.API.dll"]

# Estágio de Desenvolvimento (O que você usa no Compose)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dev
# Também instalamos aqui para o log do 'dotnet watch' ficar limpo
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app/ICR.API
COPY . /app
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:8080"]