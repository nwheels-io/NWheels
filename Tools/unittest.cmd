cd %~dp0\..\Source
dotnet test NWheels.Implementation.UnitTests\NWheels.Implementation.UnitTests.csproj
dotnet test NWheels.Injection.Adapters.Autofac.UnitTests\NWheels.Injection.Adapters.Autofac.UnitTests.csproj
dotnet test NWheels.Compilation.Adapters.Roslyn.UnitTests\NWheels.Compilation.Adapters.Roslyn.UnitTests.csproj
dotnet test NWheels.Platform.Rest.Implementation.UnitTests\NWheels.Platform.Rest.Implementation.UnitTests.csproj
dotnet test NWheels.Platform.Messaging.UnitTests\NWheels.Platform.Messaging.UnitTests.csproj
dotnet test NWheels.Frameworks.Ddd.Implementation.UnitTests\NWheels.Frameworks.Ddd.Implementation.UnitTests.csproj
pause
