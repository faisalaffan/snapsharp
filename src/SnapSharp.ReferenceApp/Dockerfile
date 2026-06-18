FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/SnapSharp.Core/SnapSharp.Core.csproj src/SnapSharp.Core/
COPY src/SnapSharp.ReferenceApp/SnapSharp.ReferenceApp.csproj src/SnapSharp.ReferenceApp/
RUN dotnet restore src/SnapSharp.ReferenceApp/SnapSharp.ReferenceApp.csproj
COPY . .
RUN dotnet publish src/SnapSharp.ReferenceApp/SnapSharp.ReferenceApp.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SnapSharp.ReferenceApp.dll"]
