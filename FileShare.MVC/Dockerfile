FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FileShare.MVC.csproj", "."]
RUN dotnet restore "./FileShare.MVC.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "FileShare.MVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileShare.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileShare.MVC.dll"]

#FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
#WORKDIR /app
#EXPOSE 80
#
#FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
#WORKDIR /src
#COPY ["FileShare.MVC/FileShare.MVC.csproj", "FileShare.MVC/"]
#COPY ["FileShare.Core/FileShare.Core.csproj", "FileShare.Core/"]
#COPY ["FileShare.Infrastructure/FileShare.Infrastructure.csproj", "FileShare.Infrastructure/"]
#RUN dotnet restore "src/Web/WebUI/WebUI.csproj"
#COPY . .
#WORKDIR "/src/src/Web/WebUI"
#RUN dotnet build "WebUI.csproj" -c Release -o /app
#
#FROM build AS publish
#RUN dotnet publish "WebUI.csproj" -c Release -o /app
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app .
#ENTRYPOINT ["dotnet", "WebUI.dll"]