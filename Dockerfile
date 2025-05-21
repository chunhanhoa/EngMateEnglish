FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Sao chép project file trước
COPY ["TiengAnh.csproj", "./"]

# Restore các package từ file .csproj (đã có Newtonsoft.Json)
RUN dotnet restore

# Sao chép toàn bộ mã nguồn
COPY . .

# Build với verbosity để debug
RUN dotnet build "TiengAnh.csproj" -c Release -o /app/build --verbosity diagnostic

# Xuất bản ứng dụng
FROM build AS publish
RUN dotnet publish "TiengAnh.csproj" -c Release -o /app/publish --no-restore --no-build

# Tạo image cuối cùng
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TiengAnh.dll"]