FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Tạo nuget.config để cấu hình nguồn package
RUN echo '<?xml version="1.0" encoding="utf-8"?>\
<configuration>\
  <packageSources>\
    <clear />\
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />\
  </packageSources>\
</configuration>' > nuget.config

# Sao chép và khôi phục project
COPY ["TiengAnh.csproj", "./"]

# Thêm package reference vào file csproj
RUN sed -i 's/<\/Project>/  <ItemGroup>\n    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" \/>\n  <\/ItemGroup>\n<\/Project>/g' TiengAnh.csproj

RUN dotnet restore "TiengAnh.csproj" --configfile nuget.config

# Sao chép toàn bộ mã nguồn và build
COPY . .
RUN dotnet build "TiengAnh.csproj" -c Release -o /app/build

# Xuất bản ứng dụng
FROM build AS publish
RUN dotnet publish "TiengAnh.csproj" -c Release -o /app/publish

# Tạo image cuối cùng
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TiengAnh.dll"]
