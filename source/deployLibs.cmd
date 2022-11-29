@echo off
cd "%~dp0"
if "%1"=="" goto:eof

copy packages\SQLitePCLRaw.lib.e_sqlite3.2.1.2\runtimes\win-x64\native %1

:eof