FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TiengAnh.csproj", "./"]
RUN dotnet restore "TiengAnh.csproj"
# Xóa bỏ file deps.json thủ công nếu có
RUN rm -f TiengAnh.deps.json || true
COPY . .
# Xóa bỏ file deps.json một lần nữa sau khi copy
RUN rm -f TiengAnh.deps.json || true
RUN dotnet add package Newtonsoft.Json --version 13.0.3
RUN dotnet build "TiengAnh.csproj" -c Release -o /app/build

FROM build AS publish
# Thêm flag bỏ qua xung đột
RUN dotnet publish "TiengAnh.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:CopyLocalLockFileAssemblies=true --no-build

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TiengAnh.dll"]
