dotnet build C:\Home\NWheels\Source\Samples\HelloWorld\NWheels.Samples.HelloWorld.Tests\NWheels.Samples.HelloWorld.Tests.csproj --no-incremental -c Debug -p:DebugType=Full -p:DebugSymbols=True

$env:NW_SYSTEST_USE_COVER = "True"
$env:NW_SYSTEST_COVER_EXE = "C:\Home\NWheels\Tools\Installed\OpenCover.4.6.519\tools\OpenCover.Console.exe"
$env:NW_SYSTEST_COVER_ARGS_TEMPLATE = '-target:dotnet.exe "-targetargs:run --project [[PROJECT]] --no-build -- [[ARGS]]" -oldStyle -register:user -filter:"+[NWheels.*]* +[*]NWheels.* -[*.*Tests]*" -excludebyattribute:*.ExcludeFromCodeCoverage* -output:C:\Temp\sys_test_cover.xml -returntargetcode:1000'
$env:NW_SYSTEST_COVER_PROJECT_PLACEHOLDER = "[[PROJECT]]"
$env:NW_SYSTEST_COVER_ARGS_PLACEHOLDER = "[[ARGS]]"

dotnet test C:\Home\NWheels\Source\Samples\HelloWorld\NWheels.Samples.HelloWorld.Tests\NWheels.Samples.HelloWorld.Tests.csproj --no-build

& "C:\Home\NWheels\Tools\Installed\ReportGenerator.3.1.2\tools\ReportGenerator.exe" -reports:C:\Temp\sys_test_cover.xml -targetdir:C:\Temp\sys_test_cover

Invoke-Item C:\Temp\sys_test_cover\index.htm
