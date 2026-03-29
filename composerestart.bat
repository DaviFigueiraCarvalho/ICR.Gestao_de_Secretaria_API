@echo off
REM ============================================================
REM Reinicia os containers usando imagens publicadas no GHCR.
REM Caso a imagem seja privada, autentique-se primeiro:
REM   docker login ghcr.io
REM ============================================================

echo [AVISO] Caso veja erro "denied", execute antes:
echo         docker login ghcr.io
echo.

REM Derruba containers que estejam em execucao
echo [INFO] Derrubando containers existentes...
docker compose down

REM Sobe novamente os containers com as imagens do registry
echo [INFO] Reiniciando os containers com as imagens publicadas...
docker compose up

pause
