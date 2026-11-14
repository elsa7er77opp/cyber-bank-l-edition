@echo off
setlocal

set "ProgramFiles=C:\Program Files"
set "ProgramFiles(x86)=C:\Program Files (x86)"
set "ProgramW6432=C:\Program Files"
set "CommonProgramFiles=C:\Program Files\Common Files"
set "CommonProgramFiles(x86)=C:\Program Files (x86)\Common Files"
set "MSBUILDDISABLENODEREUSE=1"

set "PROJECT=%~dp0src\CyberBank.LEdition.Desktop\CyberBank.LEdition.Desktop.csproj"
set "OUTPUT=%~dp0artifacts\CyberBankLEdition-win-x64"

dotnet publish "%PROJECT%" -c Release -r win-x64 --self-contained true -o "%OUTPUT%" --nologo
exit /b %ERRORLEVEL%
