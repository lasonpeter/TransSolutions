FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TransSolutions.Api/TransSolutions.Api.csproj", "TransSolutions.Api/"]
COPY ["TransSolutions.Infrastructure/TransSolutions.Infrastructure.csproj", "TransSolutions.Infrastructure/"]
COPY ["TransSolutions.Core/TransSolutions.Core.csproj", "TransSolutions.Core/"]
COPY ["TransSolutions.Shared/TransSolutions.Shared.csproj", "TransSolutions.Shared/"]

RUN dotnet restore "TransSolutions.Api/TransSolutions.Api.csproj"
COPY . .
WORKDIR "/src/TransSolutions.Api"
RUN dotnet build "TransSolutions.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TransSolutions.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TransSolutions.Api.dll"]
