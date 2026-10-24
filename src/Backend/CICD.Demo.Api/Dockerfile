# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CICD.Demo.Api.csproj ./
RUN dotnet restore CICD.Demo.Api.csproj

COPY . ./

RUN dotnet publish CICD.Demo.Api.csproj \
    --configuration Release \
    --output /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet CICD.Demo.Api.dll --urls http://0.0.0.0:${PORT:-10000}"]