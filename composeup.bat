@echo off
setlocal EnableExtensions EnableDelayedExpansion
REM ============================================================
REM Sobe os containers em modo de desenvolvimento local.
REM Usa docker-compose.override.yml para expor localhost:3000 e localhost:8080.
REM Se o GHCR negar acesso, pede login novamente e tenta subir outra vez.
REM ============================================================

echo [INFO] Sobe o ambiente local em dev.
echo         Frontend: http://localhost:3000
echo         API:      http://localhost:8080
echo.

docker info >nul 2>&1
if errorlevel 1 (
	echo [ERRO] O Docker nao esta disponivel no momento.
	echo        Abra o Docker Desktop e aguarde o Engine subir antes de executar este script.
	pause
	exit /b 1
)

echo [INFO] Derrubando containers existentes...
docker compose down
if errorlevel 1 (
	pause
	exit /b 1
)

call :StartComposeUp
if errorlevel 1 (
	pause
	exit /b 1
)

pause
exit /b 0

:StartComposeUp
echo [INFO] Subindo os containers em modo local...
set "COMPOSE_LOG=%TEMP%\composeup_%RANDOM%%RANDOM%.log"
powershell -NoProfile -Command "$log = '%COMPOSE_LOG%'; & docker compose up 2>&1 | Tee-Object -FilePath $log; exit $LASTEXITCODE"
set "COMPOSE_EXIT=!errorlevel!"
findstr /i /c:"denied" "%COMPOSE_LOG%" >nul
set "DENIED_FOUND=!errorlevel!"
type "%COMPOSE_LOG%"
del "%COMPOSE_LOG%" >nul 2>&1
if "!DENIED_FOUND!"=="0" (
	echo.
	echo [AVISO] O GitHub/GHCR negou o acesso.
	echo        Faça login novamente e tente de novo.
	docker login ghcr.io
	if errorlevel 1 exit /b 1
	echo [INFO] Tentando subir novamente...
	powershell -NoProfile -Command "$log = '%COMPOSE_LOG%'; & docker compose up 2>&1 | Tee-Object -FilePath $log; exit $LASTEXITCODE"
	set "COMPOSE_EXIT=!errorlevel!"
)
exit /b !COMPOSE_EXIT!
