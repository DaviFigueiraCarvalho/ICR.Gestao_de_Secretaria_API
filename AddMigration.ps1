param(
    [string]$migrationName
)

if ([string]::IsNullOrWhiteSpace($migrationName)) {
    $migrationName = Read-Host "Digite o nome da nova migration"
}

if ([string]::IsNullOrWhiteSpace($migrationName)) {
    Write-Host "Operação cancelada. O nome da migration não pode ser vazio." -ForegroundColor Red
    exit
}

Write-Host "Gerando migration '$migrationName'..." -ForegroundColor Cyan

# Executa o comando EF na arquitetura do seu projeto. 
# A infraestrutura guarda o DbContext; a API é o Startup Project.
dotnet ef migrations add $migrationName --project ICR.Infastructure --startup-project ICR.API

Write-Host "Comando finalizado." -ForegroundColor Green
