# Errores encontrados y correcciones

Este registro resume los problemas que bloquearon el arranque local y cómo quedaron resueltos. Usalo como checklist si el backend, Docker o el frontend vuelven a fallar.

## Checklist rápido

- [ ] Docker está iniciado si vas a usar PostgreSQL por Compose.
- [ ] Backend usa el puerto real de PostgreSQL Docker: `54329`.
- [ ] Frontend apunta al backend HTTP local: `http://localhost:5000/api`.
- [ ] Después de cambiar `frontend/.env.local`, reiniciaste Vite.
- [ ] No ejecutaste migraciones manuales si el backend ya arranca: la app automigra al iniciar.

## Registro de errores

| Error observado | Causa | Corrección aplicada | Cómo verificar |
|---|---|---|---|
| Docker instalado pero `permission denied` en `/var/run/docker.sock` o daemon inactivo. | El servicio Docker no estaba iniciado y el usuario no tenía permisos directos sobre el socket. | Iniciar y habilitar Docker con `sudo systemctl start docker` y `sudo systemctl enable docker`; usar `sudo docker compose` si el usuario aún no está en el grupo. Nota: agregar el usuario al grupo `docker` requiere cerrar sesión y volver a entrar. | `sudo docker compose ps` muestra PostgreSQL corriendo. |
| Backend intentaba conectar a PostgreSQL `5432` en vez de `54329`. | `.env.local` del backend no se cargaba automáticamente para la ejecución usada; quedaba la conexión por defecto. | Ajustar `appsettings.json`/config local para usar el puerto publicado por Docker: `54329`. | El backend arranca y `/health` responde `ok`. |
| `dotnet ef database update` fallaba por falta de `dotnet-ef`. | La herramienta CLI de EF no estaba instalada global/localmente. | No es obligatorio para esta app: `SeedData.InicializarAsync` ejecuta `Database.MigrateAsync()` al iniciar. | Ejecutar `dotnet run` en `backend`; la base se migra y se carga seed si está vacía. |
| Migración EF no discoverable. | Metadata/snapshot de migración incompleto. | Corregir metadata de migración para que EF Core la descubra. | El backend reconoce y aplica la migración inicial. |
| PostgreSQL rechazaba check constraints por columnas PascalCase. | Las constraints usaban columnas sin comillas; PostgreSQL las convertía a minúsculas. | Citar columnas PascalCase en las constraints. | La migración inicial aplica sin error SQL de columna inexistente. |
| Frontend mostraba `Network Error`. | `VITE_API_BASE_URL` apuntaba a `https://localhost:5001/api`, pero el backend escuchaba en `http://localhost:5000/api`. | Actualizar `frontend/.env.local` a `VITE_API_BASE_URL=http://localhost:5000/api` y reiniciar Vite. | Login/catálogo consumen API sin error de red. |
| Usuario lector veía `Rol: Administrador`, aparecía navegación Admin y al entrar recibía 403. | El frontend interpretaba el enum numérico `1` como administrador, aunque en backend `Lector = 1` y `Administrador = 2`. | Normalizar roles en frontend (`1 → Lector`, `2 → Administrador`) y serializar enums como texto desde el backend para nuevas respuestas. | `lector1@test.com` muestra `Rol: Lector`, no ve Admin, y una URL `/admin` manual muestra mensaje amigable de acceso restringido. |
| La disponibilidad del catálogo dependía solo de `Libro.Disponibles`. | El inventario real necesita saber qué ejemplar físico/digital se presta o reserva; los contadores pueden quedar como resumen compatible, pero no como autoridad operacional. | Agregar `Ejemplar` con código, estado, tipo y ubicación; préstamos/reservas ahora pueden apuntar a un ejemplar y el catálogo expone el detalle de copias. | `dotnet test backend.Tests/BibliotecaAPI.Tests.csproj --no-restore --verbosity minimal` valida seed, préstamo con ejemplar y rechazo sin copias disponibles. |
| El lector podía crear y devolver préstamos como si fueran checkout real. | En una biblioteca real el lector solicita; el personal aprueba, asigna ejemplar/código y procesa la devolución. | `POST /api/prestamos` ahora crea solicitud pendiente; admin usa `/api/prestamos/{id}/aprobar` y `/api/prestamos/{id}/devolver`. Reservas preparadas usan ventana de retiro de 48 h. | Tests de circulación cubren aprobación admin, bloqueo de acciones admin para lector y expiración de retiro. |
| Pagos, auditoría y notificaciones podían confundirse con integraciones reales. | La demo necesitaba comportamiento visible sin banco, email ni SMS externos. | Agregar pagos simulados en `/api/multas/{id}/pagar`, auditoría append-only en `/api/audit` y notificaciones in-app de solo lectura en `/api/notificaciones`. La UI aclara que son simulaciones. | Tests cubren aprobación/rechazo de pago, bloqueo de doble pago, recibo, eventos auditados y denegación de mutación de auditoría. |
| El catálogo no permitía validar descubrimiento real por formato, ubicación o código de ejemplar. | La búsqueda solo consideraba metadatos bibliográficos básicos y la UI no exponía señales de operación suficientes. | Extender filtros con `tipoEjemplar`/`ubicacion`, búsqueda por editorial/sinopsis/código/ubicación y etiquetas basadas en copias. La UI muestra formato, ubicación y disponibilidad reservable. | Tests backend cubren filtro digital disponible y búsqueda por código; tests frontend cubren filtro digital disponible y etiqueta de reserva. |

## Verificación recomendada

```bash
DOTNET_SYSTEM_NET_DISABLEIPV6=1 dotnet test backend.Tests/BibliotecaAPI.Tests.csproj --no-restore --verbosity minimal
cd frontend
npm test
npm run build
```

Si cambiaste `.env.local` del frontend, frená y levantá de nuevo `npm run dev`: Vite no relee esas variables en caliente.
