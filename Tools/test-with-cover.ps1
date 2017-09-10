$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Write-host "My directory is $dir"

Set-Location $dir\..\Source2

$testRunStatus = "OK"

Get-ChildItem -Directory -Recurse -Include *.UnitTests,*.IntegrationTests,*.SystemApiTests,*.SystemUITests,*.Tests | Foreach { 
    echo --- "Running test project" $_.fullName ---;     
    $dotnetArgs = '"-targetargs:test ' + $_.fullname + ' --no-build --no-restore -c Debug --filter ""(Purpose!=ManualTest)&(Purpose!=StressLoadTest)"""';
    ..\Tools\Installed\OpenCover.4.6.519\tools\OpenCover.Console.exe -target:dotnet.exe $dotnetArgs -oldStyle -register:user -filter:"+[NWheels.*]* -[*.*Tests]*" -output:$dir\TestResults\CoverageResults.xml -mergeoutput
    if ($LastExitCode -ne 0) { $testRunStatus = "FAIL" }
}

if ($testRunStatus -ne "OK") {
    throw "Some test runs FAILED"
}

..\Tools\Installed\ReportGenerator.2.5.11\tools\ReportGenerator.exe -reports:$dir\TestResults\CoverageResults.xml -targetdir:$dir\TestResults\CoverageReport

Invoke-Item $dir\TestResults\CoverageReport\index.html
