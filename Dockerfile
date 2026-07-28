FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY AuthModule.slnx ./
COPY src/AuthModule/Foundation/Foundation.csproj src/AuthModule/Foundation/
COPY src/AuthModule/CoreSecurity/CoreSecurity.csproj src/AuthModule/CoreSecurity/
COPY src/AuthModule/Governance/Governance.csproj src/AuthModule/Governance/
COPY src/AuthModule/Integration/Integration.csproj src/AuthModule/Integration/
COPY src/AuthModule/ServiceHost/ServiceHost.csproj src/AuthModule/ServiceHost/
COPY tests/AuthModule.Foundation.Tests/AuthModule.Foundation.Tests.csproj tests/AuthModule.Foundation.Tests/
COPY tests/AuthModule.CoreSecurity.Tests/AuthModule.CoreSecurity.Tests.csproj tests/AuthModule.CoreSecurity.Tests/
COPY tests/AuthModule.Governance.Tests/AuthModule.Governance.Tests.csproj tests/AuthModule.Governance.Tests/
COPY tests/AuthModule.Integration.Tests/AuthModule.Integration.Tests.csproj tests/AuthModule.Integration.Tests/
COPY tests/AuthModule.ServiceHost.Tests/AuthModule.ServiceHost.Tests.csproj tests/AuthModule.ServiceHost.Tests/
RUN dotnet restore AuthModule.slnx

COPY . .
RUN dotnet test AuthModule.slnx --configuration Release --no-restore

CMD ["dotnet", "run", "--no-launch-profile", "--project", "src/AuthModule/ServiceHost/ServiceHost.csproj", "--urls", "http://0.0.0.0:8080"]
