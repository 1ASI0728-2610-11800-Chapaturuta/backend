<#
  Deploy backend Chapaturuta a Azure Container Apps.
  Uso:  pwsh ./deploy/azure-deploy.ps1
  Requisitos: Azure CLI (az) instalado + logueado (az login).
  Lee secretos de  deploy/azure.env.local  (NO se commitea).
  Ver  plans/deploy-plan.md  paso 3.
#>

$ErrorActionPreference = "Stop"

# ---- Cargar secretos desde azure.env.local (KEY=VALUE por linea) ----
$envFile = Join-Path $PSScriptRoot "azure.env.local"
if (-not (Test-Path $envFile)) {
    throw "Falta $envFile . Copia azure.env.example -> azure.env.local y rellena valores."
}
$cfg = @{}
foreach ($line in Get-Content $envFile) {
    if ($line -match '^\s*#' -or $line -notmatch '=') { continue }
    $k, $v = $line -split '=', 2
    $cfg[$k.Trim()] = $v.Trim()
}

# ---- Parametros fijos (cambia si quieres) ----
$RG       = "rg-chapaturuta"
$ENVNAME  = "cae-chapaturuta"          # Container Apps Environment
$APP      = "frock-backend"
$LOCATION = "eastus"

Write-Host "==> Resource group" -ForegroundColor Cyan
az group create --name $RG --location $LOCATION | Out-Null

Write-Host "==> Container Apps environment (idempotente)" -ForegroundColor Cyan
az containerapp env create --name $ENVNAME --resource-group $RG --location $LOCATION 2>$null | Out-Null

Write-Host "==> Build + deploy desde Dockerfile (az containerapp up)" -ForegroundColor Cyan
# 'up' construye la imagen en la nube (ACR) y crea/actualiza el Container App.
az containerapp up `
    --name $APP `
    --resource-group $RG `
    --environment $ENVNAME `
    --source . `
    --ingress external `
    --target-port 8080

Write-Host "==> Configurar variables de entorno + secretos" -ForegroundColor Cyan
az containerapp secret set --name $APP --resource-group $RG --secrets `
    "connstr=$($cfg['ConnectionStrings__DefaultConnection'])" `
    "jwt=$($cfg['TokenSettings__Secret'])" `
    "cloud-secret=$($cfg['Cloudinary__ApiSecret'])" | Out-Null

az containerapp update --name $APP --resource-group $RG `
    --set-env-vars `
        "ASPNETCORE_ENVIRONMENT=Production" `
        "ASPNETCORE_URLS=http://+:8080" `
        "ConnectionStrings__DefaultConnection=secretref:connstr" `
        "TokenSettings__Secret=secretref:jwt" `
        "Cloudinary__CloudName=$($cfg['Cloudinary__CloudName'])" `
        "Cloudinary__ApiKey=$($cfg['Cloudinary__ApiKey'])" `
        "Cloudinary__ApiSecret=secretref:cloud-secret" `
        "GeoApi__BaseUrl=$($cfg['GeoApi__BaseUrl'])" `
        "Osrm__BaseUrl=https://router.project-osrm.org" `
        "Osrm__TimeoutSeconds=10" `
        "Osrm__Profile=driving" `
        "Cors__AllowedOrigins__0=$($cfg['FrontendOrigin'])" | Out-Null

$fqdn = az containerapp show --name $APP --resource-group $RG --query "properties.configuration.ingress.fqdn" -o tsv

# Escribe el FQDN real en el .env.production del frontend (automatico).
$envProd = Join-Path $PSScriptRoot "..\..\frontend-web\.env.production"
if (Test-Path $envProd) {
    $apiUrl = "https://$fqdn/api/"
    @(
        "# URL del backend en Azure Container Apps (escrito por azure-deploy.ps1).",
        "VITE_API_BASE_URL=$apiUrl"
    ) | Set-Content -Path $envProd -Encoding utf8
    Write-Host "Escrito frontend-web/.env.production -> $apiUrl" -ForegroundColor Green
}

Write-Host ""
Write-Host "LISTO. Backend en: https://$fqdn" -ForegroundColor Green
Write-Host "Swagger:           https://$fqdn/swagger/index.html" -ForegroundColor Green
Write-Host "Frontend .env.production ya quedo apuntando al backend. Solo falta build + deploy a Cloudflare Pages." -ForegroundColor Yellow
