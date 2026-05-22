# Biblioteca Virtual

Aplicación full-stack demo para gestión realista de biblioteca: catálogo público con ejemplares, autenticación JWT, circulación mediada por personal, reservas FIFO, multas con pagos simulados, auditoría, notificaciones in-app y administración.

## Requisitos

- .NET 10 SDK
- PostgreSQL 14+
- Node.js 18+
- Docker opcional para levantar PostgreSQL local reproducible
- Navegador Chromium si querés ejecutar Playwright

## Configuración

Copiá los ejemplos y ajustá valores locales. No commitees secretos reales.

```bash
cp .env.example .env
cp backend/.env.example backend/.env.local
cp frontend/.env.example frontend/.env.local
```

Variables clave:

- `ConnectionStrings__DefaultConnection`: conexión PostgreSQL del backend.
- `Jwt__Key`: clave local larga; cambiála fuera de demos.
- `VITE_API_BASE_URL`: URL del API para React. En la demo local actual usá `http://localhost:5000/api` y reiniciá Vite si cambiás este valor.

## Datos seed y credenciales demo

El backend migra y carga seed al iniciar si la base está vacía.

- Administrador: `admin@biblioteca.com` / `Admin123!`
- Lectores: `lector1@test.com`, `lector2@test.com`, `lector3@test.com` / `Lector123!`
- Catálogo: 20 libros, autores/categorías, ejemplares físicos/digitales con código/ubicación, préstamos activos/vencidos y reservas de ejemplo.

## Ejecutar backend

```bash
cd backend
dotnet restore
dotnet run
```

- La app aplica migraciones automáticamente al iniciar; no hace falta `dotnet ef database update` para la demo.
- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- Health check: `http://localhost:5000/health`

## Ejecutar frontend

```bash
cd frontend
npm install
npm run dev
```

- App: `http://localhost:5173`
- Rutas públicas: `/catalogo`, `/catalogo/:id`, `/login`, `/registro`
- Rutas lector: `/perfil`, `/mi-biblioteca`, `/prestamos`, `/reservas`, `/multas`, `/notificaciones`
- Rutas admin: `/admin`, `/admin/usuarios`, `/admin/catalogo`, `/admin/circulacion`, `/admin/inventario`, `/admin/auditoria`

## Verificación

```bash
DOTNET_SYSTEM_NET_DISABLEIPV6=1 dotnet test backend.Tests/BibliotecaAPI.Tests.csproj --no-restore --verbosity minimal
cd frontend
npm test
npm run build
```

Verificación reproducible de arranque de demo (PostgreSQL por Docker Compose si Docker está disponible):

```bash
./scripts/verify-demo-startup.sh
```

Este script valida backend `/health`, Swagger, migración/seed con credenciales demo, catálogo seed, build frontend y preview local.

Escaneo automatizado de configuración insegura:

```bash
./scripts/verify-config-secrets.sh
```

El escaneo falla si `.env.example`, `appsettings.json` o docs contienen secretos reales en vez de placeholders/local-demo values.

Playwright smoke, con backend y frontend ya levantados:

```bash
cd frontend
npx playwright install chromium
npm run test:e2e
```

## Arquitectura

- Backend: ASP.NET Core Web API por capas (`Controllers → Services → EF Core BibliotecaContext`).
- Frontend: React/Vite/Tailwind con `AuthContext`, servicios Axios y guards por rol.
- Reglas críticas viven en backend: límite de 3 préstamos, 14 días, bloqueo por multas/inactivo/sin stock, reservas FIFO 48 h, multas RD$50/día y permisos por rol/propietario.
- `Ejemplar` es la autoridad operativa de disponibilidad; `Libro.Stock/Disponibles` quedan como resumen compatible.
- Pagos y notificaciones son simulados para demo: no hay banco, email ni SMS reales.
- Docs de apoyo: `docs/api.md`, `docs/demo.md` y `docs/error-log.md`.

## Warnings conocidos

- En algunos entornos `dotnet restore` puede colgarse por IPv6; usá `DOTNET_SYSTEM_NET_DISABLEIPV6=1`.
- El warning NU1903 de `System.Security.Cryptography.Xml` viene transitivo de paquetes .NET/Swagger actuales; se probó referencia directa `10.0.1` y `9.0.2`, pero NuGet sigue marcándolas vulnerables. No se fuerza override inseguro: revisar cuando haya versión corregida compatible.
- Las pruebas Playwright requieren que API y frontend estén corriendo y que Chromium esté instalado.
- Si aparece un error ya visto durante el arranque local, revisá `docs/error-log.md` primero.
