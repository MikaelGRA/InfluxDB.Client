@echo off

echo. 
echo ---------------------------------------------------------------------------------------------------
echo This script updates the NuGet Binaries located in the .nuget folder in the Main directory. The
echo binaries are updated to the latest version from nuget.org
echo ---------------------------------------------------------------------------------------------------
echo.
echo ---------------------------------------------------------------------------------------------------
echo Press any key to update the binaries, or CTRL-C to cancel
echo ---------------------------------------------------------------------------------------------------
echo.

pause


..\tools\nuget\nuget.exe update -self

pause
