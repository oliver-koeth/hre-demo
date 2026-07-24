FROM mcr.microsoft.com/dotnet/sdk:10.0-preview-alpine3.20 AS build
WORKDIR /src

COPY AuthModule.slnx ./
COPY src/AuthModule/Foundation/Foundation.csproj src/AuthModule/Foundation/
COPY tests/AuthModule.Foundation.Tests/AuthModule.Foundation.Tests.csproj tests/AuthModule.Foundation.Tests/
RUN dotnet restore src/AuthModule/Foundation/Foundation.csproj && dotnet restore tests/AuthModule.Foundation.Tests/AuthModule.Foundation.Tests.csproj

COPY . .
RUN dotnet test tests/AuthModule.Foundation.Tests/AuthModule.Foundation.Tests.csproj --configuration Release --no-restore

CMD ["dotnet", "--info"]

