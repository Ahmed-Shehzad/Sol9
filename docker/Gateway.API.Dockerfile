FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY dist/gateway-api/ ./
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Gateway.API.dll"]
