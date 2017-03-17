cd %~dp0\..\Source
dotnet test NWheels.Implementation.UnitTests\NWheels.Implementation.UnitTests.csproj --logger "trx;LogFileName=test-results-1.xml"
dotnet test NWheels.Injection.Adapters.Autofac.UnitTests\NWheels.Injection.Adapters.Autofac.UnitTests.csproj --logger "trx;LogFileName=test-results-2.xml"
dotnet test NWheels.Compilation.Adapters.Roslyn.UnitTests\NWheels.Compilation.Adapters.Roslyn.UnitTests.csproj --logger "trx;LogFileName=test-results-3.xml"
dotnet test NWheels.Platform.Rest.Implementation.UnitTests\NWheels.Platform.Rest.Implementation.UnitTests.csproj --logger "trx;LogFileName=test-results-4.xml"
dotnet test NWheels.Platform.Messaging.UnitTests\NWheels.Platform.Messaging.UnitTests.csproj --logger "trx;LogFileName=test-results-5.xml"
dotnet test NWheels.Frameworks.Ddd.Implementation.UnitTests\NWheels.Frameworks.Ddd.Implementation.UnitTests.csproj --logger "trx;LogFileName=test-results-6.xml"
pause
