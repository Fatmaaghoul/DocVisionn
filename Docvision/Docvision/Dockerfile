FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy the project file
COPY ["Docvision.csproj", "."]
RUN dotnet restore "Docvision.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Docvision.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Docvision.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "Docvision.dll"]
