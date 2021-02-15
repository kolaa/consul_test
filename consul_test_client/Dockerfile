#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["consul_test_client/consul_test_client.csproj", "consul_test_client/"]
RUN dotnet restore "consul_test_client/consul_test_client.csproj"
COPY . .
WORKDIR "/src/consul_test_client"
RUN dotnet build "consul_test_client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "consul_test_client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "consul_test_client.dll"]