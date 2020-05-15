@echo off
setlocal
setlocal enabledelayedexpansion

cd /d %~dp0
set languages=cs de es pl pt ru tr
set curdate=%date:~6,4%-%date:~3,2%-%date:~0,2%
set target=%cd%\..\..\ChanSort_%curdate%
set DXversion=20.1
mkdir "%target%" 2>nul
rem del /s /q "%target%\*"
xcopy /idy debug\ChanSort.exe* "%target%"
xcopy /idy debug\ChanSort.*.dll "%target%"
xcopy /idy debug\ChanSort.ico "%target%"
xcopy /idy debug\ChanSort.*.ini "%target%"
xcopy /idy debug\Lookup.csv "%target%"
xcopy /idy DLL\* "%target%"
del "%target%\*nunit*.dll"
for %%l in (%languages%) do (
  mkdir "%target%\%%l" 2>nul
  xcopy /sidy debug\%%l "%target%\%%l"
)
mkdir "%target%\ReferenceLists" 2>nul
xcopy /sidy ChanSort\ReferenceLists\* "%target%\ReferenceLists"
xcopy /y ..\readme.md "%target%\readme.txt"
xcopy /y changelog.md "%target%\changelog.txt"
for %%f in (Utils Data Data.Desktop DataAccess Printing XtraPrinting XtraReports XtraEditors XtraBars XtraGrid XtraLayout XtraTreeList) do call :copyDll %%f
call :CodeSigning

cd ..
del Website\ChanSort.zip 2>nul
xcopy /idy readme.* %target%
cd %target%\..
"c:\program files\7-Zip\7z.exe" a -tzip ChanSort_%curdate%.zip ChanSort_%curdate%


pause
goto:eof

:CodeSigning
rem -----------------------------
rem If you want to digitally sign the generated .exe and .dll files, 
rem you need to have your code signing certificate installed in the Windows certificate storage
rem -----------------------------
set signtool="C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x64\signtool.exe"
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
%signtool% sign /a /t "http://timestamp.comodoca.com/authenticode" %todo%
:skipCodeSigning
goto:eof

:copyDll
echo Copying DevExpress %1
set source="C:\Program Files (x86)\DevExpress %DXversion%\Components\Bin\Framework\DevExpress.%1.v%DXversion%.dll"
if exist %source% xcopy /idy %source% "%target%"
set source="C:\Program Files (x86)\DevExpress %DXversion%\Components\Bin\Framework\DevExpress.%1.v%DXversion%.Core.dll"
if exist %source% xcopy /idy %source% "%target%"
for %%l in (%languages%) do call :copyLangDll %1 %%l
goto:eof

:copyLangDll
set source="C:\Program Files (x86)\DevExpress %DXversion%\Components\Bin\Framework\%2\DevExpress.%1.v%DXversion%.resources.dll"
if exist %source% xcopy /idy %source% "%target%\%2"
set source="C:\Program Files (x86)\DevExpress %DXversion%\Components\Bin\Framework\%2\DevExpress.%1.v%DXversion%.Core.resources.dll"
if exist %source% xcopy /idy %source% "%target%\%2"
goto:eof

:error
pause
goto:eof