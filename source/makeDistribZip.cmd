@echo off
cd /d %~dp0
set curdate=%date:~6,4%-%date:~3,2%-%date:~0,2%
set target=%cd%\..\..\ChanSort_%curdate%
set DXversion=19.2
mkdir "%target%" 2>nul
del /s /q "%target%\*"
copy debug\ChanSort.exe* "%target%"
copy debug\ChanSort.*.dll "%target%"
copy debug\ChanSort.ico "%target%"
copy debug\ChanSort.*.ini "%target%"
copy debug\Lookup.csv "%target%"
copy DLL\* "%target%"
del "%target%\*nunit*.dll"
mkdir "%target%\de" 2>nul
mkdir "%target%\pt" 2>nul
mkdir "%target%\ru" 2>nul
mkdir "%target%\cs" 2>nul
mkdir "%target%\es" 2>nul
mkdir "%target%\ReferenceLists" 2>nul
xcopy /siy debug\de "%target%\de"
xcopy /siy debug\pt "%target%\pt"
xcopy /siy debug\ru "%target%\ru"
xcopy /siy debug\cs "%target%\cs"
xcopy /siy debug\es "%target%\es"
xcopy /siy ChanSort\ReferenceLists\* "%target%\ReferenceLists"
copy ..\readme.md "%target%\readme.txt"
copy changelog.md "%target%\changelog.txt"
for %%f in (Utils Data DataAccess Printing XtraPrinting XtraReports XtraEditors XtraBars XtraGrid XtraLayout XtraTreeList) do call :copyDll %%f
call :CodeSigning

cd ..
del Website\ChanSort.zip 2>nul
copy Source\readme.txt %target%
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
set files=ChanSort.exe ChanSort*.dll de\ChanSort*.dll ru\ChanSort*.dll pt\ChanSort*.dll cs\ChanSort*.dll es\ChanSort*.dll
%signtool% sign /a /t "http://timestamp.comodoca.com/authenticode" %files%
if errorlevel 1 goto :error
cd %oldcd%
goto:eof

:copyDll
echo Copying DevExpress %1
set source="C:\Program Files (x86)\DevExpress %DXversion%\Components\Bin\Framework\DevExpress.%1.v%DXversion%.dll"
if exist %source% copy %source% "%target%"
set source="C:\Program Files (x86)\DevExpress %DXversion%\Components\Bin\Framework\DevExpress.%1.v%DXversion%.Core.dll"
if exist %source% copy %source% "%target%"
for %%l in (de pt) do call :copyLangDll %1 %%l
goto:eof

:copyLangDll
set source="C:\Program Files (x86)\DevExpress %DXversion%\Components\Bin\Framework\%2\DevExpress.%1.v%DXversion%.resources.dll"
if exist %source% copy %source% "%target%\%2"
set source="C:\Program Files (x86)\DevExpress %DXversion%\Components\Bin\Framework\%2\DevExpress.%1.v%DXversion%.Core.resources.dll"
if exist %source% copy %source% "%target%\%2"
goto:eof

:error
pause
goto:eof