@echo off

echo ABOUT TO MERGE HAPIL PROJECTS INTO NWHEELS.SLN
echo HIT ANY KEY TO PROCEED OR CTRL+C TO ABORT
echo - - - - - - - - - - - - - - - - - - - - - - - - - - -

pause

cd /D %~dp0

..\..\Source\Bin\Core\ntool.exe merge-solution --source ..\..\..\Hapil\Source\Hapil.sln --target ..\..\Source\NWheels.sln --projects hapil-projects-to-merge.txt

pause
