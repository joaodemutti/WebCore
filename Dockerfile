# Use the official ASP.NET runtime image as a base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy the .csproj and restore dependencies
COPY ["WebCore/WebCore.csproj", "WebCore/"]
RUN dotnet restore "WebCore/WebCore.csproj"

# Copy the entire project and build the app
COPY . .
RUN dotnet publish "WebCore/WebCore.csproj" -c Release -o /app/publish -p:UseAppHost=false

# Use the runtime image to run the app
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebCore.dll"]
