FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Sao chép và khôi phục project
COPY ["TiengAnh.csproj", "./"]
# Thêm packages cần thiết trực tiếp
RUN dotnet add package Newtonsoft.Json --version 13.0.3
RUN dotnet restore "TiengAnh.csproj" --verbosity normal

# Sao chép toàn bộ mã nguồn và build với debug output
COPY . .
RUN dotnet build "TiengAnh.csproj" -c Release -o /app/build --verbosity detailed

# Xuất bản ứng dụng
FROM build AS publish
RUN dotnet publish "TiengAnh.csproj" -c Release -o /app/publish

# Tạo image cuối cùng
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TiengAnh.dll"]
