# Estágio de Runtime (Final)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# Precisamos ser root para instalar a lib
USER root 
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
# Voltamos para o usuário seguro
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ... (estágios build e publish permanecem iguais) ...

# Estágio de Desenvolvimento (O que você usa no Compose)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dev
# Também instalamos aqui para o log do 'dotnet watch' ficar limpo
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app/ICR.API
COPY . /app
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:8080"]
# Estágio Final de Produção
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ICR.API.dll"]