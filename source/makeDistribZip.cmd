@echo off
setlocal
setlocal enabledelayedexpansion

cd /d %~dp0
set languages=cs de es hu it pl pt ro ru tr
set curdate=%date:~6,4%-%date:~3,2%-%date:~0,2%
set target=%cd%\..\..\ChanSort_%curdate%
set DXversion=23.2
set framework=net48
set bindir=debug\%framework%

mkdir "%target%" 2>nul
rem del /s /q "%target%\*"
xcopy /idy %bindir%\ChanSort.exe* "%target%"
xcopy /idy %bindir%\ChanSort.*.dll "%target%"
xcopy /idy %bindir%\ChanSort.*.ini "%target%"

xcopy /idy %bindir%\Microsoft.*.dll "%target%"
xcopy /idy %bindir%\SQLitePCLRaw.*.dll "%target%"
mkdir "%target%\runtimes" 2>nul
xcopy /idys packages\SQLitePCLRaw.lib.e_sqlite3.2.1.2\runtimes\win-arm "%target%\runtimes\win-arm"
xcopy /idys packages\SQLitePCLRaw.lib.e_sqlite3.2.1.2\runtimes\win-arm64 "%target%\runtimes\win-arm64"
xcopy /idys packages\SQLitePCLRaw.lib.e_sqlite3.2.1.2\runtimes\win-x86 "%target%\runtimes\win-x86"
xcopy /idys packages\SQLitePCLRaw.lib.e_sqlite3.2.1.2\runtimes\win-x64 "%target%\runtimes\win-x64"
xcopy /idy %bindir%\SharpCompress.dll "%target%"
xcopy /idy %bindir%\System.*.dll "%target%"
xcopy /idy %bindir%\Newtonsoft.Json.dll "%target%"
xcopy /idy %bindir%\Lookup.csv "%target%"
xcopy /idy DLL\* "%target%"
del "%target%\*nunit*.dll"
for %%l in (%languages%) do (
  mkdir "%target%\%%l" 2>nul
  xcopy /sidy %bindir%\%%l\ChanSort.* "%target%\%%l"
)
mkdir "%target%\ReferenceLists" 2>nul
xcopy /sidy ChanSort\ReferenceLists\* "%target%\ReferenceLists"
copy /y ..\readme.md "%target%\readme.txt"
copy /y changelog.md "%target%\changelog.txt"
for %%f in (Data Data.Desktop DataAccess Drawing Pdf Printing Utils XtraBars XtraEditors XtraGrid XtraLayout XtraPrinting XtraReports XtraTreeList) do call :copyDll %%f
call :CodeSigning

cd ..
del Website\ChanSort.zip 2>nul
xcopy /idy readme.* %target%
cd %target%\..
"c:\program files\7-Zip\7z.exe" a -tzip -mx9 ChanSort_%curdate%.zip ChanSort_%curdate%


pause
goto:eof

:CodeSigning
rem -----------------------------
rem If you want to digitally sign the generated .exe and .dll files, 
rem you need to have your code signing certificate installed in the Windows certificate storage
rem -----------------------------
for /d %%f in ("C:\Program Files (x86)\Windows Kits\10\bin\10.*") do (
  set nq=%%f
  set nq=!nq:"=!
  set signtool="!nq!\x86\signtool.exe"
  if exist !signtool! goto foundSigntool
)

echo "can't find signtool.exe"
pause
goto:eof
:foundSigntool

set oldcd=%cd%
cd %target%
call :signBatch ChanSort.exe ChanSort*.dll
if errorlevel 1 goto :error
set files=
for %%l in (%languages%) do (
  call :signBatch "%%l\ChanSort*.dll"
  if errorlevel 1 goto :error
)
cd %oldcd%
goto:eof
:signBatch
set todo=
for %%f in (%*) do (
  %signtool% verify /pa "%%f" >nul 2>nul
  if errorlevel 1 set todo=!todo! "%%f"
)
if "%todo%" == "" goto:skipCodeSigning
%signtool% sign /n "ABPro Entwicklungs-, Vertriebs- und Wartungs GmbH" /tr "http://timestamp.digicert.com" /td SHA256 %todo%
:skipCodeSigning
goto:eof

:copyDll
echo Copying DevExpress %1
for %%s in (.dll .Core.dll .Drawing.dll) do call :copyDllFramework %1 %%s
for %%l in (%languages%) do call :copyLangDll %1 %%l
goto:eof

:copyDllFramework
for %%f in (Framework Standard NetCore) do (
  set source="C:\Program Files\DevExpress %DXversion%\Components\Bin\%%f\DevExpress.%1.v%DXversion%%2"
  if exist !source! xcopy /iy !source! "%target%" & goto:eof
)
goto:eof


:copyLangDll
for %%s in (.resources.dll .Core.resources.dll) do call :copyLangDllFramework %1 %%s
set source="c:\daten\downloads\DevExpress\20%DXversion%\DevExpressLocalizedResources_20%DXversion%_%2\DevExpress.%1.v%DXversion%.resources.dll"
if exist !source! xcopy /idy !source! "%target%\%2"
set source="c:\daten\downloads\DevExpress\20%DXversion%\DevExpressLocalizedResources_20%DXversion%_%2\DevExpress.%1.v%DXversion%.Core.resources.dll"
if exist !source! xcopy /idy !source! "%target%\%2"
goto:eof

:copyLangDllFramework
for %%f in (Framework Standard NetCore) do (
  set source="C:\Program Files\DevExpress %DXversion%\Components\Bin\%%f\%2\DevExpress.%1.v%DXversion%%2"
  if exist !source! xcopy /idy !source! "%target%\%2" & goto:eof
)
goto:eof

:error
pause
goto:eof