FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Cài đặt Newtonsoft.Json trước vào image
RUN dotnet new console -n temp && \
    cd temp && \
    dotnet add package Newtonsoft.Json --version 13.0.3 && \
    cd ..

# Sao chép và khôi phục project
COPY ["TiengAnh.csproj", "./"]
RUN dotnet restore "TiengAnh.csproj"

# Sao chép mã nguồn và build
COPY . .
RUN dotnet build "TiengAnh.csproj" -c Release -o /app/build

# Xuất bản ứng dụng
FROM build AS publish
RUN dotnet publish "TiengAnh.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Tạo image cuối cùng
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TiengAnh.dll"]
