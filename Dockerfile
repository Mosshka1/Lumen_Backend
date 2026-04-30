FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "./Lumen.API/Lumen.API.csproj"
RUN dotnet publish "./Lumen.API/Lumen.API.csproj" -c Release -o /app/publish --no-restore

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Lumen.API.dll"]