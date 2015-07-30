@echo off

echo ABOUT TO UNMERGE HAPIL PROJECTS FROM NWHEELS.SLN
echo HIT ANY KEY TO PROCEED OR CTRL+C TO ABORT
echo - - - - - - - - - - - - - - - - - - - - - - - - - - -

pause

cd /D %~dp0

..\..\Source\Bin\Core\ntool.exe merge-solution --unmerge --source ..\..\..\Hapil\Source\Hapil.sln --target ..\..\Source\NWheels.sln --projects hapil-projects-to-merge.txt

pause
