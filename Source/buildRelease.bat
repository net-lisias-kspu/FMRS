

@echo off


copy bin\%1\FMRS.dll ..\GameData\FMRS\Plugins
copy ..\license.txt ..\GameData\FMRS

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"

copy FlightManagerforReusableStages.version a.version
set VERSIONFILE=a.version
rem The following requires the JQ program, available here: https://stedolan.github.io/jq/download/
c:\local\jq-win64  ".VERSION.MAJOR" %VERSIONFILE% >tmpfile
set /P major=<tmpfile

c:\local\jq-win64  ".VERSION.MINOR"  %VERSIONFILE% >tmpfile
set /P minor=<tmpfile

c:\local\jq-win64  ".VERSION.PATCH"  %VERSIONFILE% >tmpfile
set /P patch=<tmpfile

c:\local\jq-win64  ".VERSION.BUILD"  %VERSIONFILE% >tmpfile
set /P build=<tmpfile
del tmpfile
set VERSION=%major%.%minor%.%patch%
if "%build%" NEQ "0"  set VERSION=%VERSION%.%build%

echo %VERSION%
del a.version
copy FlightManagerforReusableStages.version ..\GameData\FMRS

set FILE="%RELEASEDIR%\FMRS-%VERSION%-%1.zip"
IF EXIST %FILE% del /F %FILE%
cd ..
%ZIP% a -tzip %FILE% GameData\FMRS  GameData\RecoveryController
