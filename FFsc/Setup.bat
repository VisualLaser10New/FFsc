@ECHO OFF

echo Before execute me read the readme or the docs at https://github.com/VisualLaser10New/FFsc#readme
pause

taskkill /im FFscw.exe /f
taskkill /im ffsc.exe /f

rmdir /S /Q ".\Data"
del /Q "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\ffscw*"


(echo cd "%cd%\" && echo.:loop && echo."%cd%\FFscw.exe" && echo.goto loop) > "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\ffscw.bat"

reg add "HKCU\Environment" /v Path /t REG_SZ /d "%PATH%;%cd%\FFsc.exe;" /f

:rerun
FFscw.exe -r
goto :rerun

REM kill all ffscw.exe and ffsc.exe already opened
REM delete Data folder
REM delete from PATH ffsc.exe
REM delete all links from shell:startup about ffscw

REM create link of ffscw.exe in shell:startup
REM add to PATH path of ffsc.exe
REM start "ffscw.exe -r" if it stops restart it