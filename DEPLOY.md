# Deploy en CasaOS con Cloudflare Tunnel

Guía para correr la Biblioteca Virtual en un servidor casero (CasaOS / Linux con Docker) exponiéndolo a internet por túnel de Cloudflare. **Sin abrir puertos en el router, sin exponer tu IP pública.**

## Arquitectura

```
Internet
   │
   ▼
┌──────────────────────┐
│  Cloudflare edge     │  (HTTPS + DNS)
│  *.alvaroacevedo.dev │
└──────────┬───────────┘
           │ túnel saliente (cloudflared)
           ▼
┌─────────────────────────────────────────────┐
│  Tu CasaOS (red docker `biblioteca`)        │
│                                             │
│  cloudflared ──► frontend (nginx :80)       │
│              └─► backend  (.NET    :8080)   │
│                       │                     │
│                       ▼                     │
│                  postgres :5432             │
└─────────────────────────────────────────────┘
```

Tres reglas de ruteo en el túnel:
- `biblioteca.alvaroacevedo.dev`     → `http://frontend:80`
- `api.biblioteca.alvaroacevedo.dev` → `http://backend:8080`
- catch-all                          → `http_status:404`

## Paso 1 — Clonar el repo en tu CasaOS

```bash
cd /DATA/AppData          # o donde guardes proyectos en CasaOS
git clone https://github.com/varocode/biblioteca-virtual.git
cd biblioteca-virtual
```

## Paso 2 — Crear el túnel en Cloudflare

1. Entrá a [dash.cloudflare.com](https://dash.cloudflare.com) → **Zero Trust** (menú lateral)
2. Si es tu primera vez, te pedirá crear un Team (gratis, plan Free).
3. En Zero Trust: **Networks → Tunnels → Create a tunnel**.
4. Elegí **Cloudflared**, nombre: `biblioteca-casa`.
5. **Save tunnel**. En la pantalla siguiente Cloudflare muestra el comando con un token largo. **NO lo corras manualmente** — solo copiá el valor del token (lo que viene después de `--token`). Lo vamos a meter en `.env.prod`.

   El token se ve así (es muy largo, base64):
   ```
   eyJhIjoiYWJjMTIzLi4uLi4uLi4uLi4uLi4uLi4uLi4uLi4uLi4u...
   ```

6. Click **Next**. Acá configurás las rutas públicas. Agregá:

   | Subdomain    | Domain                | Service type | URL                |
   |--------------|-----------------------|--------------|---------------------|
   | `biblioteca` | `alvaroacevedo.dev`   | `HTTP`       | `frontend:80`       |
   | `api.biblioteca` | `alvaroacevedo.dev` | `HTTP`     | `backend:8080`      |

   El primero (frontend) lo creás con **Add a public hostname**. Después dale a **Add a public hostname** otra vez para el segundo (api).

   *Nota*: el "Service URL" usa nombres de contenedor (`frontend`, `backend`) porque cloudflared corre dentro de la misma red docker que los servicios.

7. **Save tunnel**. Listo, el túnel está creado pero apagado hasta que arrancás el container.

## Paso 3 — Configurar las variables

```bash
cp env.prod.example .env.prod
nano .env.prod  # o el editor que prefieras
```

Llenar:

```bash
POSTGRES_PASSWORD=$(openssl rand -base64 32)
JWT_KEY=$(openssl rand -base64 48)
FRONTEND_URL=https://biblioteca.alvaroacevedo.dev
VITE_API_BASE_URL=https://api.biblioteca.alvaroacevedo.dev/api
CLOUDFLARE_TUNNEL_TOKEN=<el token largo que copiaste arriba>
```

*Tip*: corré `openssl rand -base64 32` y `openssl rand -base64 48` en tu CasaOS y pegá los valores en el archivo.

## Paso 4 — Levantar todo

```bash
docker compose --env-file .env.prod -f docker-compose.prod.yml up -d --build
```

La primera vez tarda **5-10 minutos** (descarga imágenes, compila .NET, build de Vite). Las siguientes son segundos.

Verificar que todo arrancó:

```bash
docker compose --env-file .env.prod -f docker-compose.prod.yml ps
```

Deberías ver 4 contenedores en estado `Up` o `healthy`:
- biblioteca-postgres
- biblioteca-backend
- biblioteca-frontend
- biblioteca-tunnel

## Paso 5 — Verificar

1. Abrí `https://biblioteca.alvaroacevedo.dev` → debería cargar el catálogo.
2. Probá login con un usuario seed. Mirá `backend/Data/SeedData.cs` para ver las credenciales sembradas.
3. Si algo no carga, revisar logs:

   ```bash
   # Logs del backend (ahí ves errores de DB, JWT, etc.)
   docker compose --env-file .env.prod -f docker-compose.prod.yml logs -f backend

   # Logs del túnel (ahí ves si las rutas se conectaron)
   docker compose --env-file .env.prod -f docker-compose.prod.yml logs -f cloudflared
   ```

## Actualizar después de hacer cambios

Cada vez que pushees al repo:

```bash
git pull
docker compose --env-file .env.prod -f docker-compose.prod.yml up -d --build
```

Solo se reconstruye lo que cambió.

## Backup de la base de datos

```bash
docker compose --env-file .env.prod -f docker-compose.prod.yml exec postgres \
  pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB" > backup-$(date +%Y%m%d).sql
```

## Troubleshooting

**"connection refused" entre containers** → Verificá que estén todos en la network `biblioteca`. Lista con: `docker network inspect biblioteca-virtual_biblioteca`.

**El frontend carga pero el login da error de red** → El frontend está pegándole al backend con la URL equivocada. Mirá `VITE_API_BASE_URL` en `.env.prod` y reconstruí el frontend: `docker compose ... up -d --build frontend`.

**Error de CORS** → Revisá que `FRONTEND_URL` en `.env.prod` coincida EXACTAMENTE con el dominio que abrís en el browser (incluido `https://`).

**EF Core: "no se puede conectar"** → El backend arrancó antes que Postgres esté healthy. Reiniciá: `docker compose ... restart backend`.

**Túnel "disconnected" en el dashboard de Cloudflare** → Mirá `docker logs biblioteca-tunnel`. Lo más común: token mal copiado.
