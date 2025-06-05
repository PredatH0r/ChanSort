@echo off
cd "%~dp0"
if "%1"=="" goto:eof

rem this script is deactivated

rem copy packages\SQLitePCLRaw.lib.e_sqlite3.2.1.2\runtimes\win-x64\native %1

goto :eof
mkdir %1\runtimes\win-x64\native 2>nul
copy %userprofile%\.nuget\packages\SQLitePCLRaw.lib.e_sqlite3\2.1.10\runtimes\win-x64\native %1\runtimes\win-x64\native
copy %userprofile%\.nuget\packages\System.Memory\4.6.3\lib\net462\*.dll %1

:eof