FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Api/PaymentGateway.Api/PaymentGateway.Api.csproj", "Api/PaymentGateway.Api/"]
COPY ["Core/PaymentGateway.Application/PaymentGateway.Application.csproj", "Core/PaymentGateway.Application/"]
COPY ["Core/PaymentGateway.Domain/PaymentGateway.Domain.csproj", "Core/PaymentGateway.Domain/"]
COPY ["Core/PaymentGateway.Shared/PaymentGateway.Shared.csproj", "Core/PaymentGateway.Shared/"]
COPY ["Infrastructure/PaymentGateway.Persistence/PaymentGateway.Persistence.csproj", "Infrastructure/PaymentGateway.Persistence/"]
COPY ["Infrastructure/PaymentGateway.Api/PaymentGateway.Api.csproj", "Api/PaymentGateway.Api/"]

RUN dotnet restore "Api/PaymentGateway.Api/PaymentGateway.Api.csproj"
COPY . .

RUN dotnet tool install --global dotnet-ef --version 9.0.0
RUN /root/.dotnet/tools/dotnet-ef database update

WORKDIR "/src/Api/PaymentGateway.Api"
RUN dotnet build "PaymentGateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PaymentGateway.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PaymentGateway.Api.dll"]