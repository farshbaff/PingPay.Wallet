#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Wallet.Api/WalletApi.csproj", "Wallet.Api/"]
RUN dotnet restore "Wallet.Api/WalletApi.csproj"
COPY . .
WORKDIR "/src/Wallet.Api"
RUN dotnet build "WalletApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WalletApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WalletApi.dll"]