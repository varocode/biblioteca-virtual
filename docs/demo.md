# Guía de demo

## Preparación

1. Para prueba reproducible, ejecutá `./scripts/verify-demo-startup.sh` desde la raíz. El script puede levantar PostgreSQL con Docker Compose, iniciar backend, validar Swagger/seed, construir frontend y servir preview.
2. Para demo manual, levantá PostgreSQL (`docker compose up -d postgres`) y configurá `backend/.env.local` o variables equivalentes.
3. Ejecutá backend: `dotnet run --project backend`.
4. Ejecutá frontend: `cd frontend && npm run dev`.
5. Abrí `http://localhost:5173` y Swagger en `http://localhost:5000/swagger`.

## Recorrido sugerido

### Visitante
- Entrá a `/catalogo`.
- Buscá “Demostración”, filtrá por categoría, disponibilidad o formato digital y abrí el detalle de un libro.
- En el detalle revisá código de ejemplar, estado, formato y ubicación. No hay lector de código de barras real; el código es dato demo.

### Lector
- Login: `lector1@test.com` / `Lector123!`.
- Revisá `/mi-biblioteca`, `/prestamos`, `/reservas`, `/multas` y `/notificaciones`.
- Solicitá préstamo de un libro disponible: queda pendiente hasta que administración lo apruebe.
- Reservá un libro sin disponibilidad: administración puede prepararlo para retiro con ventana de 48 h.
- Probá pagar una multa desde `/multas`: el botón aprueba/rechaza una pasarela simulada, sin banco real.

### Administrador
- Login: `admin@biblioteca.com` / `Admin123!`.
- Revisá `/admin` para métricas y gráficos.
- En `/admin/usuarios`, activá/desactivá un lector.
- En `/admin/catalogo`, creá, editá y eliminá libros/autores/categorías de prueba.
- En `/admin/inventario`, verificá ejemplares por código, estado, formato y ubicación.
- En `/admin/circulacion`, aprobá solicitudes, procesá devoluciones y prepará reservas para retiro.
- En `/admin/auditoria`, revisá eventos append-only de circulación, reservas, multas y pagos.

## Límites simulados

- Pagos: determinísticos por botón demo; no se contacta ninguna pasarela.
- Notificaciones: inbox interno de solo lectura; no hay email/SMS/background worker.
- Códigos de ejemplar: son strings generados por seed/alta de catálogo; no hay hardware de barcode.
- Roles: `Administrador` ve rutas admin; `Lector` recibe mensaje amable en UI y `403` en API si fuerza URLs admin.

## Checklist de cierre

- Backend tests pasan.
- Frontend tests pasan.
- Frontend build genera `dist/`.
- Playwright smoke corre con API/frontend activos.
- No hay secretos reales en `.env.example` ni docs: `./scripts/verify-config-secrets.sh` pasa.
