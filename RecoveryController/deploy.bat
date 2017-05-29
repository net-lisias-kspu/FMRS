
@echo off
echo "RecoveryController deploy"

set H=R:\KSP_1.3.0_dev
echo %H%

copy bin\%1\RecoveryController.dll ..\GameData\RecoveryController\Plugins

