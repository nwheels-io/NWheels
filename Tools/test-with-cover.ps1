param (
    [string]$subFolder = ""
)

$scriptPath = $MyInvocation.MyCommand.Path
$scriptDir = Split-Path $scriptPath
Write-host "Script directory is $scriptDir"

$topDir = "$scriptDir\..\Source2";

If ($subFolder) {
    $topDir = "$topDir\$subFolder";
}

Write-host "Top test directory is $topDir"

$testRunStatus = "OK"

Remove-Item -Recurse -Force $topDir\TestResults
New-Item -ItemType Directory -Force -Path $topDir\TestResults | Out-Null

Get-ChildItem -Path $topDir -Directory -Recurse -Include *.UnitTests,*.IntegrationTests,*.SystemApiTests,*.SystemUITests,*.Tests | Foreach { 
    echo --- "Running test project" $_.fullName ---;     
    $dotnetArgs = '"-targetargs:test ' + $_.fullname + ' --no-build --no-restore -c Debug --filter ""(Purpose!=ManualTest)&(Purpose!=StressLoadTest)"""';
    & $scriptDir\Installed\OpenCover.4.6.519\tools\OpenCover.Console.exe -target:dotnet.exe $dotnetArgs -oldStyle -register:user -filter:"+[NWheels.*]* -[*.*Tests]*" -excludebyattribute:*.ExcludeFromCodeCoverage* -output:$topDir\TestResults\CoverageResults.xml -mergeoutput
    if ($LastExitCode -ne 0) { $testRunStatus = "FAIL" }
}

if ($testRunStatus -ne "OK") {
    throw "Some test runs FAILED"
}

& "$scriptDir\Installed\ReportGenerator.2.5.11\tools\ReportGenerator.exe" -reports:$topDir\TestResults\CoverageResults.xml -targetdir:$topDir\TestResults\CoverageReport

Invoke-Item $topDir\TestResults\CoverageReport\index.htm
