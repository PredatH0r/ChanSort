@echo off
set target=%cd%\..\Program
set DXversion=12.2
mkdir "%target%" 2>nul
del /s /q "%target%\*"
copy debug\ChanSort.exe* "%target%"
copy debug\ChanSort.*.dll "%target%"
copy debug\ChanSort.*.ini "%target%"
copy debug\Lookup.csv "%target%"
copy DLL\*.dll "%target%"
mkdir "%target%\de" 2>nul
xcopy /siy debug\de "%target%\de"
copy readme.txt "%target%"
for %%f in (Utils Data XtraEditors XtraBars XtraGrid XtraLayout) do call :copyDll %%f

cd ..
del Website\ChanSort.zip 2>nul
copy Source\readme.txt %target%
cd %target%
"c:\program files (x86)\7-Zip\7z.exe" a -tzip ChanSort.zip *


pause
goto:eof

:copyDll
echo Copying DevExpress %*
set source="C:\Program Files (x86)\DevExpress\DXperience %DXversion%\Bin\Framework\DevExpress.%*.v%DXversion%.dll"
if exist %source% copy %source% "%target%"
set source="C:\Program Files (x86)\DevExpress\DXperience %DXversion%\Bin\Framework\DevExpress.%*.v%DXversion%.Core.dll"
if exist %source% copy %source% "%target%"
set source="C:\Program Files (x86)\DevExpress\DXperience %DXversion%\Bin\Framework\de\DevExpress.%*.v%DXversion%.resources.dll"
if exist %source% copy %source% "%target%\de"
set source="C:\Program Files (x86)\DevExpress\DXperience %DXversion%\Bin\Framework\de\DevExpress.%*.v%DXversion%.Core.resources.dll"
if exist %source% copy %source% "%target%\de"
goto:eof