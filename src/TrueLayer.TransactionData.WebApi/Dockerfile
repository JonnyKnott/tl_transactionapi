FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS source
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY TrueLayer.TransactionData.sln ./
COPY src/TrueLayer.TransactionData.WebApi/TrueLayer.TransactionData.WebApi.csproj src/TrueLayer.TransactionData.WebApi/
COPY src/TrueLayer.TransactionData.Services/TrueLayer.TransactionData.Services.csproj src/TrueLayer.TransactionData.Services/
COPY src/TrueLayer.TransactionData.Models/TrueLayer.TransactionData.Models.csproj src/TrueLayer.TransactionData.Models/
COPY test/TrueLayer.TransactionData.WebApi.Integration.Test/TrueLayer.TransactionData.WebApi.Integration.Test.csproj test/TrueLayer.TransactionData.WebApi.Integration.Test/
COPY test/TrueLayer.TransactionData.Services.Test/TrueLayer.TransactionData.Services.Test.csproj test/TrueLayer.TransactionData.Services.Test/
RUN dotnet restore

WORKDIR /src
COPY . .

FROM source as publish
WORKDIR /src/src/TrueLayer.TransactionData.WebApi
RUN dotnet build -c Release -o /app
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 as base
WORKDIR /ap
COPY --from=publish /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "TrueLayer.TransactionData.WebApi.dll"]