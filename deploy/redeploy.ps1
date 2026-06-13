<#
  Redeploy del backend a Azure Container Apps.
  Uso:  pwsh ./deploy/redeploy.ps1
  Reconstruye la imagen desde el codigo ACTUAL y actualiza la app.
  NO toca secretos/env vars (persisten del create original).

  Requisitos: az CLI logueado (az login). DB Aiven encendida (free tier duerme).
#>

$ErrorActionPreference = "Continue"

# Bypass de inspeccion SSL de Avast (si esta activo). El interceptor es tu propio AV.
$env:AZURE_CLI_DISABLE_CONNECTION_VERIFICATION = "1"
$env:REQUESTS_CA_BUNDLE = ""

# ---- Parametros (no cambian) ----
$ACR = "ca44395bb318acr"
$RG  = "rg-chapaturuta"
$APP = "frock-backend"
$SRC = Split-Path $PSScriptRoot -Parent          # carpeta backend/
$TAG = Get-Date -Format "yyyyMMddHHmmss"          # tag unico -> fuerza nueva revision
$IMG = "$ACR.azurecr.io/${APP}:$TAG"

# 1) Staging liviano: copia el codigo SIN datos pesados ni secretos.
#    Sin esto, az subiria ~2.2 GB (osm-data + osrm-data) y se cuelga.
Write-Host "==> Preparando staging (sin osm/osrm/bin/obj/secretos)..." -ForegroundColor Cyan
$stage = "$env:TEMP\frock-build"
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Path $stage | Out-Null
robocopy $SRC $stage /E `
    /XD osm-data osrm-data bin obj logs .git .vs node_modules deploy .idea .claude Frock-backend.Tests plans docs Properties `
    /XF "*.pbf" "*.osrm*" /NFL /NDL /NJH /NJS /NP | Out-Null
$mb = [math]::Round((Get-ChildItem $stage -Recurse -File | Measure-Object Length -Sum).Sum/1MB, 2)
Write-Host "    staging = $mb MB" -ForegroundColor DarkGray

# 2) Build de la imagen en la nube (ACR). Respeta el Dockerfile multi-stage.
Write-Host "==> Build imagen $TAG en ACR..." -ForegroundColor Cyan
az acr build --registry $ACR --image "${APP}:$TAG" --file "$stage\Dockerfile" $stage
if ($LASTEXITCODE -ne 0) { throw "az acr build fallo (exit $LASTEXITCODE)" }

# 3) Apuntar el Container App a la nueva imagen (crea nueva revision, manteniendo env/secretos).
Write-Host "==> Actualizando Container App a la imagen nueva..." -ForegroundColor Cyan
az containerapp update -n $APP -g $RG --image $IMG
if ($LASTEXITCODE -ne 0) { throw "az containerapp update fallo (exit $LASTEXITCODE)" }

$fqdn = az containerapp show -n $APP -g $RG --query "properties.configuration.ingress.fqdn" -o tsv
Write-Host ""
Write-Host "LISTO. Backend actualizado (tag $TAG)." -ForegroundColor Green
Write-Host "  https://$fqdn/swagger/index.html" -ForegroundColor Green
Write-Host "  Verifica: https://$fqdn/health" -ForegroundColor Green
