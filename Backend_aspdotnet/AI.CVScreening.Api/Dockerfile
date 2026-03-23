FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AI.CVScreening.Api.csproj", "./"]
RUN dotnet restore "AI.CVScreening.Api.csproj"

COPY . .
RUN dotnet publish "AI.CVScreening.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AI.CVScreening.Api.dll"]
