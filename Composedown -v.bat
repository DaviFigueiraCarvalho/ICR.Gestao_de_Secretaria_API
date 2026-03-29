@echo off
REM ============================================================
REM Derruba os containers E remove os volumes de dados.
REM ATENCAO: os dados do banco serao apagados permanentemente.
REM ============================================================

REM Para os containers e remove volumes (inclusive dados do banco)
echo [AVISO] Esta operacao remove todos os volumes, incluindo dados do banco!
echo [INFO] Derrubando os containers e removendo volumes...
docker compose down -v

pause
