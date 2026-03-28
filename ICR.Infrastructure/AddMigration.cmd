@echo off
chcp 65001 > nul
setlocal

set /p migName="Digite o nome da nova migration: "

if "%migName%"=="" (
    echo Operação cancelada. O nome da migration não pode ser vazio.
    pause
    exit /b 1
)

echo Gerando migration "%migName%"...

dotnet ef migrations add "%migName%" 
if %errorlevel% neq 0 (
    echo Erro ao gerar migration. Abortando.
    pause
    exit /b 1
)

echo Aplicando migration no banco...

dotnet ef database update
if %errorlevel% neq 0 (
    echo Erro ao atualizar o banco.
    pause
    exit /b 1
)

echo Tudo certo. 
pause