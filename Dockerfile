FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Api/PaymentGateway.Api/PaymentGateway.Api.csproj", "Api/PaymentGateway.Api/"]
COPY ["src/Core/PaymentGateway.Application/PaymentGateway.Application.csproj", "Core/PaymentGateway.Application/"]
COPY ["src/Core/PaymentGateway.Domain/PaymentGateway.Domain.csproj", "Core/PaymentGateway.Domain/"]
COPY ["src/Core/PaymentGateway.Shared/PaymentGateway.Shared.csproj", "Core/PaymentGateway.Shared/"]
COPY ["src/Infrastructure/PaymentGateway.Persistence/PaymentGateway.Persistence.csproj", "Infrastructure/PaymentGateway.Persistence/"]
COPY ["src/Infrastructure/PaymentGateway.ApiClient/PaymentGateway.ApiClient.csproj", "Infrastructure/PaymentGateway.ApiClient/"]

RUN dotnet restore "Api/PaymentGateway.Api/PaymentGateway.Api.csproj"
COPY . .

WORKDIR /src
RUN dotnet build "src/Api/PaymentGateway.Api/PaymentGateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/Api/PaymentGateway.Api/PaymentGateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PaymentGateway.Api.dll"]