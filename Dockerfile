FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Tạo userrepo.cs.fixed để sửa lỗi
RUN mkdir -p Repositories

# Sao chép project file trước
COPY ["TiengAnh.csproj", "./"]

# Thêm packages cần thiết trực tiếp
RUN dotnet add package Newtonsoft.Json --version 13.0.3
RUN dotnet add package MongoDB.Driver --version 2.22.0

# Cập nhật và restore
RUN dotnet restore

# Sao chép toàn bộ mã nguồn
COPY . .

# Thêm bước sửa lỗi cho UserRepository.cs nếu cần
RUN if grep -q "Newtonsoft" ./Repositories/UserRepository.cs; then \
    echo "using Newtonsoft.Json;" > temp_file && \
    cat ./Repositories/UserRepository.cs >> temp_file && \
    mv temp_file ./Repositories/UserRepository.cs; \
    fi

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
