@echo off
REM ============================================================
REM Sobe os containers usando imagens publicadas no GHCR.
REM Caso a imagem seja privada, autentique-se primeiro:
REM   docker login ghcr.io
REM ============================================================

echo [AVISO] Caso veja erro "denied", execute antes:
echo         docker login ghcr.io
echo.

REM Derruba containers que estejam em execucao
echo [INFO] Derrubando containers existentes...
docker compose down

REM Sobe os containers baixando as imagens do registry
echo [INFO] Subindo os containers com as imagens publicadas...
docker compose up

pause
