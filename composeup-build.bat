@echo off
setlocal EnableExtensions

REM Garante que os caminhos relativos sejam resolvidos a partir desta pasta.
pushd "%~dp0"
set "COMPOSE_FILE=docker-compose.local.yml"

REM ============================================================
REM Recria o ambiente local com build e acompanha os logs.
REM Sequencia:
REM   1. docker compose down
REM   2. docker compose up --build -d
REM   3. docker compose up
REM ============================================================

echo [INFO] Recriando o ambiente local com %COMPOSE_FILE%.
echo         API: http://localhost:8080
echo.

docker info >nul 2>&1
if errorlevel 1 (
    echo [ERRO] O Docker nao esta disponivel no momento.
    echo        Abra o Docker Desktop e aguarde o Engine subir antes de executar este script.
    popd
    pause
    exit /b 1
)

echo [1/3] Derrubando os containers existentes...
docker compose -f "%COMPOSE_FILE%" down
if errorlevel 1 goto :error

echo.
echo [2/3] Construindo e iniciando os containers em segundo plano...
docker compose -f "%COMPOSE_FILE%" up --build -d
if errorlevel 1 goto :error

echo.
echo [3/3] Acompanhando os containers e exibindo os logs...
echo       Pressione Ctrl+C para encerrar o compose up.
docker compose -f "%COMPOSE_FILE%" up
set "COMPOSE_EXIT=%ERRORLEVEL%"

popd
if not "%COMPOSE_EXIT%"=="0" (
    echo.
    echo [ERRO] O docker compose up terminou com o codigo %COMPOSE_EXIT%.
    pause
    exit /b %COMPOSE_EXIT%
)

echo.
echo [INFO] Execucao finalizada.
pause
exit /b 0

:error
set "COMPOSE_EXIT=%ERRORLEVEL%"
echo.
echo [ERRO] O comando Docker Compose falhou com o codigo %COMPOSE_EXIT%.
popd
pause
exit /b %COMPOSE_EXIT%
