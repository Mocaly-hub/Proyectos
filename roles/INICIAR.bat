@echo off
cd /d "%~dp0"
py manage.py migrate
if errorlevel 1 goto error
py manage.py runserver
goto end
:error
echo.
echo No se pudo iniciar el proyecto. Revisa que estes usando el Python de la clase con Django configurado.
pause
:end
