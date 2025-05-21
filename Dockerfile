FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TiengAnh.csproj", "./"]
COPY ["TiengAnh.deps.json", "./"]
RUN dotnet restore "TiengAnh.csproj"
COPY . .
RUN dotnet add package Newtonsoft.Json --version 13.0.3
RUN dotnet build "TiengAnh.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TiengAnh.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TiengAnh.dll"]
