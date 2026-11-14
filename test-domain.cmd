@echo off
set "ProgramFiles=C:\Program Files"
set "ProgramFiles(x86)=C:\Program Files (x86)"
set "ProgramW6432=C:\Program Files"
set "CommonProgramFiles=C:\Program Files\Common Files"
set "CommonProgramFiles(x86)=C:\Program Files (x86)"
set "MSBUILDDISABLENODEREUSE=1"
cd /d "%~dp0"
dotnet restore tests\CyberBank.LEdition.Domain.Tests\CyberBank.LEdition.Domain.Tests.csproj --configfile NuGet.Config --verbosity minimal
if errorlevel 1 exit /b %errorlevel%
dotnet test tests\CyberBank.LEdition.Domain.Tests\CyberBank.LEdition.Domain.Tests.csproj --no-restore --logger "console;verbosity=normal"
