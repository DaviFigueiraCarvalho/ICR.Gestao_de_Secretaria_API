# Script para gerenciar a aplicação localmente com Docker Compose
# Uso: .\docker-local.ps1 [comando]

param(
	[Parameter(Mandatory = $false, Position = 0)]
	[ValidateSet("start", "stop", "restart", "logs", "build", "clean", "status", "shell", "help")]
	[string]$Comando = "help"
)

$composeFl = "docker-compose.local.yml"
$apiService = "icr.api"
$dbService = "postgres"

function Show-Help {
	Write-Host "
╔════════════════════════════════════════════════════════════════╗
║         Docker Compose Local - Gerenciador de Aplicação       ║
╚════════════════════════════════════════════════════════════════╝

Uso: .\docker-local.ps1 [comando]

COMANDOS DISPONÍVEIS:
  start      - Inicia os containers (build + up)
  stop       - Para os containers
  restart    - Reinicia os containers
  logs       - Mostra logs em tempo real
  build      - Reconstrói a imagem da API
  clean      - Remove containers e volumes (reset completo)
  status     - Mostra status dos containers
  shell      - Conecta ao shell da API
  help       - Mostra esta mensagem

EXEMPLOS:
  .\docker-local.ps1 start       # Inicia a aplicação
  .\docker-local.ps1 logs        # Mostra logs
  .\docker-local.ps1 clean       # Limpa tudo
  .\docker-local.ps1             # Mostra esta ajuda

URLS LOCAIS:
  API:      http://localhost:8080
  Swagger:  http://localhost:8080/swagger
  PgAdmin:  http://localhost:5050
  DB:       localhost:5432

CREDENCIAIS:
  PgAdmin - Email: admin@localhost | Password: admin123
  DB - User: icradmin | Password: root
	" -ForegroundColor Cyan
}

function Start-Application {
	Write-Host "🚀 Iniciando aplicação..." -ForegroundColor Green
	Write-Host "   Construindo imagens..." -ForegroundColor Gray
	docker-compose -f $composeFl build --no-cache

	Write-Host "   Iniciando containers..." -ForegroundColor Gray
	docker-compose -f $composeFl up -d

	Write-Host "✅ Aplicação iniciada!" -ForegroundColor Green
	Write-Host "   API:      http://localhost:8080" -ForegroundColor Yellow
	Write-Host "   PgAdmin:  http://localhost:5050" -ForegroundColor Yellow
	Write-Host ""
	Write-Host "💡 Dica: Use '.\docker-local.ps1 logs' para ver os logs" -ForegroundColor Cyan
}

function Stop-Application {
	Write-Host "⏹️  Parando aplicação..." -ForegroundColor Yellow
	docker-compose -f $composeFl stop
	Write-Host "✅ Aplicação parada!" -ForegroundColor Green
}

function Restart-Application {
	Write-Host "🔄 Reiniciando aplicação..." -ForegroundColor Yellow
	docker-compose -f $composeFl restart
	Write-Host "✅ Aplicação reiniciada!" -ForegroundColor Green
}

function Show-Logs {
	Write-Host "📋 Mostrando logs (CTRL+C para sair)..." -ForegroundColor Cyan
	docker-compose -f $composeFl logs -f
}

function Build-Api {
	Write-Host "🔨 Reconstruindo imagem da API..." -ForegroundColor Yellow
	docker-compose -f $composeFl build --no-cache $apiService
	Write-Host "✅ Imagem reconstruída!" -ForegroundColor Green
}

function Clean-Application {
	Write-Host "🗑️  Limpando aplicação (removendo containers e volumes)..." -ForegroundColor Red
	$confirm = Read-Host "Tem certeza? (s/n)"

	if ($confirm -eq 's' -or $confirm -eq 'S') {
		docker-compose -f $composeFl down -v
		Write-Host "✅ Aplicação limpa!" -ForegroundColor Green
		Write-Host "💡 Use '.\docker-local.ps1 start' para iniciar novamente" -ForegroundColor Cyan
	}
	else {
		Write-Host "Operação cancelada." -ForegroundColor Yellow
	}
}

function Show-Status {
	Write-Host "📊 Status dos containers:" -ForegroundColor Cyan
	docker-compose -f $composeFl ps
}

function Open-Shell {
	Write-Host "🐚 Abrindo shell da API..." -ForegroundColor Cyan
	docker-compose -f $composeFl exec $apiService sh
}

# Executar comando
switch ($Comando) {
	"start" { Start-Application }
	"stop" { Stop-Application }
	"restart" { Restart-Application }
	"logs" { Show-Logs }
	"build" { Build-Api }
	"clean" { Clean-Application }
	"status" { Show-Status }
	"shell" { Open-Shell }
	default { Show-Help }
}
