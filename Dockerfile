FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["DoAnCoSo/DoAnCoSo.csproj", "DoAnCoSo/"]
RUN dotnet restore "DoAnCoSo/DoAnCoSo.csproj"
COPY . .
WORKDIR "/src/DoAnCoSo"
RUN dotnet build "DoAnCoSo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DoAnCoSo.csproj" -c Release -o /app/publish

FROM publish AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DoAnCoSo.dll"]
