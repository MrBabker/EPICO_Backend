FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# نسخ ملف المشروع مباشرة
COPY epico_backend.csproj ./
RUN dotnet restore "./epico_backend.csproj"

# نسخ باقي الملفات
COPY . ./
RUN dotnet publish -c Release -o out

# المرحلة الثانية - runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out ./

EXPOSE 8080
ENTRYPOINT ["dotnet", "epico_backend.dll"]