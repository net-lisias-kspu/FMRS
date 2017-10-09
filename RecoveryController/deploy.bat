
@echo off
echo "RecoveryController deploy"

set H=R:\KSP_1.3.1_dev
echo %H%

copy /Y "%2%3" "..\GameData\%GAMEDIR%\Plugins"
