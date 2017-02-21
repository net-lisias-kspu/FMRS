

cd
pause
set H=R:\KSP_1.2.2_dev
echo %H%

copy bin\%1\FMRS.dll ..\GameData\FMRS\Plugins
xcopy /E /Y ..\GameData\FMRS %H%\GameData\FMRS


pause