# Plan de Despliegue — Chapaturuta (gratis + Azure $100 de reserva)

Objetivo: backend, frontend y base de datos en producción **sin gastar** (o casi) los $100 de Azure.

## Arquitectura final

```
   Usuario
     │
     ▼
┌──────────────────────┐        ┌──────────────────────────────┐
│  Cloudflare Pages    │  HTTPS │  Azure Container Apps         │
│  (frontend Vue/Vite) │ ─────► │  (backend ASP.NET Core 9)     │
│  *.pages.dev   FREE  │  /api  │  free grant mensual           │
└──────────────────────┘        └───────────────┬──────────────┘
                                                 │ MySQL + SSL
                                                 ▼
                                  ┌──────────────────────────────┐
                                  │  Aiven MySQL   FREE always-on │
                                  └──────────────────────────────┘
   Externos ya gratis: Cloudinary (imágenes), OSRM demo público (routing).
```

| Pieza | Servicio | Costo | Por qué |
|---|---|---|---|
| Frontend | Cloudflare Pages | Gratis siempre | SPA estática, bandwidth ilimitado |
| Backend | Azure Container Apps | Free grant (no toca $100) | Docker nativo, escala a 0 |
| DB | Aiven MySQL free | Gratis | MySQL real, compatible EF Core 9 |

Los $100 de Azure quedan **de reserva** para cuando Aiven se quede corto de storage → migrar a Azure DB for MySQL Flexible.

---

## ⚠️ Seguridad

El repo tenía **secretos reales expuestos** (password DB Railway + `Cloudinary__ApiSecret` en texto plano). Estado:

### Ya ejecutado (limpieza del repo) ✅
- `backend/render.yaml` → **borrado** (tenía password DB Railway + `Cloudinary__ApiSecret` en texto plano).
- `backend/appsettings.json` → Cloudinary `ApiKey`/`ApiSecret` **vaciados** (ahora van por env en Azure).
- `backend/.gitignore` → ignora `deploy/azure.env.local`.
- Verificado: cero secretos restantes en el repo.

### Pendiente — manual, HAZLO ANTES DE DESPLEGAR ⛔
**Rotar la llave de Cloudinary** (está fuera del repo, servicio externo). El secret viejo (`l7Z4...`) ya circuló en archivos commiteados → quemado. Pasos:
1. https://console.cloudinary.com → **Settings → API Keys** (o **Security**).
2. Generar nueva API Key + Secret.
3. Ponerlas en `backend/deploy/azure.env.local` (`Cloudinary__ApiKey`, `Cloudinary__ApiSecret`).
4. Revocar/eliminar la llave vieja.

> La DB de Railway ya no se usa → ese password se ignora.

### Regla permanente
Nunca poner secretos en archivos commiteados (`render.yaml`, `appsettings*.json`, `.env`). Usar siempre `deploy/azure.env.local` (en `.gitignore`) o env vars del Container App.

---

## Paso 1 — Base de datos: Aiven MySQL

1. Crear cuenta en https://aiven.io (free, sin tarjeta).
2. **Create service → MySQL → plan Free**. Región más cercana (ej. AWS `us-east`).
3. Esperar ~2 min a que quede `RUNNING`.
4. En la pestaña **Connection information** copiar: Host, Port, User (`avnadmin`), Password, Database (`defaultdb`).
5. Armar el connection string (provider **Oracle** `MySql.EntityFrameworkCore`, **SSL obligatorio**):

   ```
   Server=HOST;Port=PORT;Database=defaultdb;User Id=avnadmin;Password=PASS;SslMode=Required
   ```
   > Nota: el backend usa el provider de Oracle (`UseMySQL`), no Pomelo. Las keys son `Server` / `User Id` / `SslMode` (no `user=` estilo Pomelo).

> El backend corre `EnsureCreated` al arrancar → crea las tablas solo. El seed geográfico (1694 distritos) carga del snapshot embebido. Cero SQL manual.

---

## Paso 2 — Preparar secretos del backend

1. Copiar plantilla:
   ```powershell
   cd backend
   Copy-Item deploy/azure.env.example deploy/azure.env.local
   ```
2. Editar `deploy/azure.env.local`:
   - `ConnectionStrings__DefaultConnection` → el de Aiven (paso 1).
   - `TokenSettings__Secret` → string aleatorio largo. Generar:
     ```powershell
     [Convert]::ToBase64String((1..48 | % { Get-Random -Max 256 }))
     ```
   - `Cloudinary__*` → las llaves **nuevas** (rotadas).
   - `FrontendOrigin` → lo dejas como `https://chapaturuta.pages.dev` por ahora; lo ajustas en paso 5.

> `azure.env.local` está en `.gitignore`. No se sube.

---

## Paso 3 — Backend: Azure Container Apps

### Requisitos
- Azure CLI: https://aka.ms/installazurecli
- Login: `az login`
- Extensión: `az extension add --name containerapp --upgrade`
- Proveedores: `az provider register --namespace Microsoft.App; az provider register --namespace Microsoft.OperationalInsights`

### Desplegar
```powershell
cd backend
pwsh ./deploy/azure-deploy.ps1
```

El script:
1. Crea resource group + Container Apps environment.
2. `az containerapp up --source .` → construye la imagen Docker **en la nube** (no necesitas Docker local) y la despliega.
3. Inyecta variables de entorno y secretos (connstr, JWT, Cloudinary como `secretref`).
4. Imprime la **URL pública** del backend (`https://frock-backend.....azurecontainerapps.io`).

### Verificar
- Swagger: `https://<fqdn>/swagger/index.html`
- Health: `https://<fqdn>/health`

> **Escala a 0:** Container Apps apaga el contenedor sin tráfico → primer request tras inactividad tarda unos segundos (cold start). Para demo con jurado, sube el mínimo a 1 réplica:
> ```powershell
> az containerapp update -n frock-backend -g rg-chapaturuta --min-replicas 1
> ```
> (Consume un poco del free grant; sigue siendo gratis para uso chico.)

---

## Paso 4 — Frontend: Cloudflare Pages

El proyecto es un **monorepo**; el frontend vive en `frontend-web/`.

1. Editar `frontend-web/.env.production` → poner el FQDN real del backend (paso 3):
   ```
   VITE_API_BASE_URL=https://frock-backend-....azurecontainerapps.io/api/
   ```
   (Con `/api/` y slash final — así lo concatena `base-service.js`.)

2. Crear cuenta en https://dash.cloudflare.com → **Workers & Pages → Create → Pages → Connect to Git**.

3. Conectar el repo de GitHub y configurar el build:

   | Campo | Valor |
   |---|---|
   | Root directory | `frontend-web` |
   | Framework preset | `Vue` (o None) |
   | Build command | `pnpm install && pnpm build` |
   | Build output directory | `dist` |
   | Variable de entorno | `VITE_API_BASE_URL` = `https://<fqdn-azure>/api/` |

4. **Save and Deploy**. Sale una URL tipo `https://chapaturuta.pages.dev`.

> `frontend-web/public/_redirects` (`/* /index.html 200`) ya está creado → arregla las rutas de Vue Router (`createWebHistory`) para que `F5` en `/login` no dé 404.

---

## Paso 5 — Conectar CORS (cerrar el círculo)

El backend solo acepta requests del frontend si su dominio está en CORS. Ya es **configurable por env** (editado en `Program.cs`).

1. Apunta el backend al dominio real de Pages:
   ```powershell
   az containerapp update -n frock-backend -g rg-chapaturuta `
     --set-env-vars "Cors__AllowedOrigins__0=https://chapaturuta.pages.dev"
   ```
   (Si Pages te dio otro nombre, usa ese.)

2. Si usas dominio custom además del `.pages.dev`, agrega `Cors__AllowedOrigins__1=...`.

---

## Checklist final

- [ ] Cloudinary rotado, secretos fuera del repo
- [ ] Aiven MySQL `RUNNING`, connection string con `SslMode=Required`
- [ ] `deploy/azure.env.local` rellenado (NO commiteado)
- [ ] Backend desplegado, Swagger responde
- [ ] `.env.production` con el FQDN de Azure
- [ ] Cloudflare Pages buildeando desde `frontend-web/`
- [ ] CORS del backend incluye el dominio de Pages
- [ ] Abrir el frontend → login → request al backend OK (revisar Network tab, sin errores CORS)

---

## Diagnóstico rápido

| Síntoma | Causa probable | Fix |
|---|---|---|
| Frontend: error CORS en consola | dominio Pages no está en CORS | Paso 5 |
| 404 al recargar una ruta | falta `_redirects` | ya creado; revisa que esté en `dist/` |
| Backend 500 al arrancar | connection string mal / sin SSL | revisar `SslMode=Required` |
| Backend tarda 1ra vez | cold start (escala a 0) | `--min-replicas 1` |
| Rutas/ETA dan 502 | OSRM demo con rate limit | esperar; degrada con gracia (no rompe CRUD) |
| `az containerapp up` falla | falta extensión/proveedor | ver Requisitos paso 3 |

---

## Costos reales

- Cloudflare Pages: **$0**.
- Aiven free MySQL: **$0** (límite ~1 GB storage).
- Azure Container Apps: dentro del free grant mensual (180k vCPU-s + 360k GiB-s + 2M req) → **$0** para uso de demo/clase. Solo con `--min-replicas 1` 24/7 podrías rozar el límite; aun así barato.
- Los **$100 Azure intactos** como plan B para mover la DB a Azure MySQL Flexible (~$12-15/mes) si Aiven queda corto.
