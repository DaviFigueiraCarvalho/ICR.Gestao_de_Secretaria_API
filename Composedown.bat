@echo off
REM ============================================================
REM Derruba os containers sem remover os volumes de dados.
REM ============================================================

REM Para os containers e remove a rede criada pelo compose
echo [INFO] Derrubando os containers...
docker compose down

pause
