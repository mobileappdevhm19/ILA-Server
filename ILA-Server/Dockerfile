FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["ILA-Server/ILA-Server.csproj", "ILA-Server/"]
RUN dotnet restore "ILA-Server/ILA-Server.csproj"
COPY . .
WORKDIR "/src/ILA-Server"
RUN dotnet build "ILA-Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ILA-Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ILA-Server.dll"]