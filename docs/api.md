# API rápida

Base local recomendada para la demo: `http://localhost:5000/api`. Swagger: `http://localhost:5000/swagger`.

## Autenticación

- `POST /auth/register` — registra lector y devuelve JWT.
- `POST /auth/login` — devuelve `{ token, expiresAt, usuario }`.
- `GET /auth/me` — usuario autenticado.

Usá `Authorization: Bearer <token>` para endpoints protegidos.

## Catálogo

- `GET /libros?search=&autorId=&categoriaId=&anio=&disponible=&tipoEjemplar=&ubicacion=&page=&pageSize=&sortBy=&sortDir=`
- `GET /libros/{id}`
- `GET /autores`, `GET /categorias`
- Admin: `POST/PUT/DELETE /libros`, `/autores`, `/categorias`

El catálogo busca también por editorial, sinopsis, código de ejemplar y ubicación. `tipoEjemplar` acepta `Fisico` o `Digital`; `sortBy` acepta `titulo`, `anio`, `autor`, `categoria`, `editorial`, `disponibilidad` y `recientes`. Los DTOs devuelven `ejemplares`, `formatos`, `ubicaciones` y `etiquetaDisponibilidad` basados en copias reales.

Validaciones principales: ISBN único y `disponibles <= stock`. Al crear/editar, el stock genera ejemplares demo con código y ubicación.

## Lectores

- `PUT /usuarios/me` — perfil propio.
- `PUT /usuarios/me/password` — cambio de contraseña.
- `GET/POST /prestamos` — listar/solicitar préstamo pendiente de aprobación.
- `POST /prestamos/{id}/renovar`
- `GET/POST/DELETE /reservas`
- `GET /multas`, `POST /multas/{id}/pagar` — intento de pago simulado.
- `GET /notificaciones` — inbox in-app de solo lectura.

Reglas: máximo 3 préstamos activos, 14 días, reserva FIFO 48h y multa RD$50 por día vencido. Pagos y notificaciones son simulados: no hay banco, email ni SMS reales.

## Administración

- `GET /usuarios`, `PUT /usuarios/{id}` — gestión de usuarios.
- `GET /dashboard/resumen` — métricas, top libros, usuarios activos, préstamos por mes y categorías populares.
- `GET /ejemplares?libroId=` — inventario operativo con código, estado, tipo y ubicación.
- `POST /prestamos/{id}/aprobar`, `POST /prestamos/{id}/devolver` — checkout/check-in por personal.
- `POST /reservas/{id}/preparar-retiro` — prepara retiro y abre ventana de 48 h.
- `GET /audit?entidad=&usuarioId=` — auditoría append-only; `PUT/DELETE /audit/{id}` devuelven `405`.

Lectores reciben `403` en endpoints admin; usuarios sin JWT reciben `401`. Las acciones de lector siguen chequeando propiedad en backend.
