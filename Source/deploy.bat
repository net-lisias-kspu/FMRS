
set H=R:\KSP_1.3.0_dev
echo %H%

copy bin\%1\FMRS.dll ..\GameData\FMRS\Plugins
xcopy /E /Y ..\GameData\FMRS %H%\GameData\FMRS
