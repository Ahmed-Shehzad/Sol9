FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet build Sol9.AppHost/Sol9.AppHost.csproj -c Release -p:BuildProjectReferences=false
RUN mkdir -p /app/publish && cp -R Sol9.AppHost/bin/Release/net10.0/* /app/publish/

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src /src
COPY --from=build /root/.nuget /root/.nuget
ENV ASPNETCORE_URLS=http://0.0.0.0:18888
ENV ASPIRE_ALLOW_UNSECURED_TRANSPORT=true
EXPOSE 18888 4317 4318
ENTRYPOINT ["dotnet", "Sol9.AppHost.dll"]
