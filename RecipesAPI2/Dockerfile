FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /ktu_receptai
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /ktu_receptai/RecipesAPI2
COPY ["RecipesAPI2.csproj", "./"]
RUN dotnet restore "RecipesAPI2.csproj"
COPY . .
WORKDIR "/ktu_receptai/RecipesAPI2/"
RUN dotnet build "RecipesAPI2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RecipesAPI2.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RecipesAPI2.dll"]
