Write-host "NOTE: this script must run with Administrator privileges"
Write-host "TODO: make this script actually reusable by NWheels community"
Write-host "TODO: (1) make all paths relative to this script location"
Write-host "TODO: (2) check if Chocolatey and NuGet are installed and install only what is missing"
Write-host "TODO: (3) provide a shell version of this script for Linux devs"

# install Chocolatey
Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

# install OpenCover
nuget install OpenCover -Version 4.6.519 -OutputDirectory c:\oss\NWheels\Tools\Installed\OpenCover.4.6.519

# install ReportGenerator
nuget install ReportGenerator -Version 3.1.2 -OutputDirectory c:\oss\NWheels\Tools\Installed\ReportGenerator.3.1.2


