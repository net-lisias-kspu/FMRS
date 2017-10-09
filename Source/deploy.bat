
@echo off

set H=R:\KSP_1.3.1_dev
set GAMEDIR=FMRS

echo %H%

copy /Y "%2%3" "..\GameData\%GAMEDIR%\Plugins"
copy /Y FlightManagerforReusableStages.version ..\GameData\%GAMEDIR%

cd ..
cd
xcopy /y /s /I  GameData\%GAMEDIR% "%H%\GameData\%GAMEDIR%"

pause