#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM node:18.15.0-alpine3.17 AS nodejs
WORKDIR /src
COPY ["frontend", "frontend/"]
COPY ["browser_client", "browser_client/"]
RUN npm ci --prefix browser_client && npm run build --prefix browser_client
RUN npm ci --prefix frontend
WORKDIR /src/frontend
RUN npx ng build


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["MemoNotify/MemoNotify.csproj", "MemoNotify/"]
RUN dotnet restore "MemoNotify/MemoNotify.csproj"
COPY . .
COPY --from=nodejs /src/frontend/dist/frontend /src/MemoNotify/wwwroot/
WORKDIR "/src/MemoNotify"
RUN dotnet build "MemoNotify.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "MemoNotify.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM busybox AS cert
RUN wget https://s3.amazonaws.com/rds-downloads/rds-combined-ca-bundle.pem

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=cert rds-combined-ca-bundle.pem /etc/ssl/certs/
ENTRYPOINT ["dotnet", "MemoNotify.dll"]