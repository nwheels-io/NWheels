param (
    [string]$subFolder = "",
    [switch]$unitIntegTests,
    [switch]$systemTests,
    [switch]$buildSolution,
    [switch]$buildProjects
)

$scriptPath = $MyInvocation.MyCommand.Path
$scriptDir = Split-Path $scriptPath
Write-host "Script directory is $scriptDir"

$topDir = "$scriptDir\..\Source";

If ($subFolder) {
    $topDir = "$topDir\$subFolder";
}

Write-host "Top test directory is $topDir"

$testRunStatus = "OK"

Remove-Item -Recurse -Force $topDir\TestResults
New-Item -ItemType Directory -Force -Path $topDir\TestResults | Out-Null

if ($buildSolution) {
	& dotnet build "$scriptDir\..\Source\NWheels.sln" --no-incremental -c Debug -p:DebugType=Full -p:DebugSymbols=True
}

if ($unitIntegTests) {
	Get-ChildItem -Path $topDir -Directory -Recurse -Include *.UnitTests,*.IntegrationTests,*.Tests | Foreach { 
		echo --- "Running Unit+Integration tests in project" $_.fullName ---;     

		if ($buildProjects) {
			& dotnet build $_.fullname --no-incremental -c Debug -p:DebugType=Full -p:DebugSymbols=True
		}

		$dotnetArgs = '"-targetargs:test ' + $_.fullname + ' --no-build --no-restore -c Debug --filter ""(Purpose=UnitTest)|(Purpose=IntegrationTest)"""';
		& $scriptDir\Installed\OpenCover.4.6.519\tools\OpenCover.Console.exe -target:dotnet.exe $dotnetArgs -oldStyle -register:user -filter:"+[NWheels.*]* +[*]NWheels.* -[*.*Tests]*" -excludebyattribute:*.ExcludeFromCodeCoverage* -output:$topDir\TestResults\CoverageResults.xml -mergeoutput -returntargetcode:1000

		if ($LastExitCode -ne 0) { $testRunStatus = "FAIL" }
	}
}

if ($systemTests) {
	$env:NW_SYSTEST_USE_COVER = "True"
	$env:NW_SYSTEST_COVER_EXE = "$scriptDir\Installed\OpenCover.4.6.519\tools\OpenCover.Console.exe"
	$env:NW_SYSTEST_COVER_ARGS_TEMPLATE = '-target:dotnet.exe "-targetargs:run --project [[PROJECT]] --no-build -- [[ARGS]]" -oldStyle -register:user -filter:"+[NWheels.*]* +[*]NWheels.* -[*.*Tests]*" -excludebyattribute:*.ExcludeFromCodeCoverage* ' + "-output:$topDir\TestResults\CoverageResults.xml -mergeoutput -returntargetcode:1000"
	$env:NW_SYSTEST_COVER_PROJECT_PLACEHOLDER = "[[PROJECT]]"
	$env:NW_SYSTEST_COVER_ARGS_PLACEHOLDER = "[[ARGS]]"

	Get-ChildItem -Path $topDir -Directory -Recurse -Include *.SystemApiTests,*.SystemUITests,*.Tests | Foreach { 
		echo --- "Running System API+UI tests in project" $_.fullName ---;     

		if ($buildProjects) {
			& dotnet build $_.fullname --no-incremental -c Debug -p:DebugType=Full -p:DebugSymbols=True
		}

		& dotnet test $_.fullname --no-build -c Debug --filter "(Purpose=SystemApiTest)|(Purpose=SystemUITest)"

		if ($LastExitCode -ne 0) { $testRunStatus = "FAIL" }
	}
}

if ($testRunStatus -ne "OK") {
    throw "Some test runs FAILED"
}

& "$scriptDir\Installed\ReportGenerator.3.1.2\tools\ReportGenerator.exe" -reports:$topDir\TestResults\CoverageResults.xml -targetdir:$topDir\TestResults\CoverageReport

Invoke-Item $topDir\TestResults\CoverageReport\index.htm
