# PRD — Sistema de Gestión de Biblioteca Virtual

> **Product Requirements Document**
> Trabajo Final · Taller de Programación I · UNICARIBE · Grupo 1

---

## 📋 Tabla de Contenido

1. [Resumen Ejecutivo](#1-resumen-ejecutivo)
2. [Objetivos del Proyecto](#2-objetivos-del-proyecto)
3. [Stack Tecnológico](#3-stack-tecnológico)
4. [Arquitectura del Sistema](#4-arquitectura-del-sistema)
5. [Modelo de Datos](#5-modelo-de-datos)
6. [Reglas de Negocio](#6-reglas-de-negocio)
7. [Backend — API REST](#7-backend--api-rest)
8. [Frontend — Aplicación React](#8-frontend--aplicación-react)
9. [Autenticación y Seguridad](#9-autenticación-y-seguridad)
10. [Estructura de Carpetas](#10-estructura-de-carpetas)
11. [Datos de Prueba (Seed)](#11-datos-de-prueba-seed)
12. [Instrucciones de Instalación](#12-instrucciones-de-instalación)
13. [Criterios de Aceptación](#13-criterios-de-aceptación)
14. [Entregables](#14-entregables)

---

## 1. Resumen Ejecutivo

### 1.1 Descripción del Producto

Sistema web full-stack de gestión de biblioteca virtual que permite a usuarios consultar un catálogo de libros, realizar préstamos, hacer reservas y administrar su perfil. Incluye un panel administrativo completo para bibliotecarios con gestión de inventario, usuarios, préstamos, multas y reportes estadísticos.

### 1.2 Contexto del Proyecto

- **Asignatura:** Taller de Programación I
- **Universidad:** UNICARIBE (Universidad del Caribe)
- **Modalidad:** Trabajo grupal final (Grupo 1)
- **Audiencia objetivo:** Estudiantes universitarios, bibliotecarios, administradores

### 1.3 Alcance del Proyecto

✅ **Incluido:**
- Aplicación web responsive
- Backend API REST con autenticación JWT
- Base de datos relacional persistente
- Panel de usuario lector
- Panel administrativo completo
- Sistema de préstamos con reglas de negocio
- Sistema de reservas y multas
- Dashboard con estadísticas

❌ **No incluido (fuera de alcance):**
- Pagos electrónicos reales
- Notificaciones por email/SMS
- Lectura digital de libros (e-reader)
- Aplicación móvil nativa
- Integración con sistemas externos

---

## 2. Objetivos del Proyecto

### 2.1 Objetivo General

Desarrollar un sistema web completo de gestión de biblioteca virtual que automatice los procesos de catalogación, préstamo, reserva y seguimiento de libros, mejorando la experiencia de usuarios y administradores.

### 2.2 Objetivos Específicos

1. Diseñar una interfaz web moderna, responsive y accesible utilizando React y Tailwind CSS.
2. Implementar una API REST robusta en C# .NET que gestione toda la lógica del negocio.
3. Aplicar estructuras condicionales, repetitivas y de control en la lógica del backend.
4. Utilizar Entity Framework Core para mapear objetos a una base de datos PostgreSQL.
5. Generar reportes estadísticos visuales en el dashboard administrativo.
6. Implementar autenticación segura con JWT y manejo de roles (Lector / Administrador).
7. Aplicar reglas de negocio reales como límites de préstamo, cálculo de multas y cola de reservas.

---

## 3. Stack Tecnológico

### 3.1 Backend

| Componente | Tecnología | Versión |
|---|---|---|
| Lenguaje | C# | 12+ |
| Framework | ASP.NET Core Web API | .NET 10 |
| ORM | Entity Framework Core | 10 |
| Base de datos | PostgreSQL | 14+ |
| Autenticación | JWT Bearer | — |
| Hash de contraseñas | BCrypt.Net-Next | — |
| Documentación API | Swagger / Swashbuckle | — |
| Validación | DataAnnotations + FluentValidation | — |
| Logging | Serilog | — |
| CORS | Microsoft.AspNetCore.Cors | — |

### 3.2 Frontend

| Componente | Tecnología | Versión |
|---|---|---|
| Framework | React | 18+ |
| Bundler | Vite | 5+ |
| Estilos | Tailwind CSS | 3+ |
| Routing | React Router DOM | 6+ |
| HTTP Client | Axios | 1+ |
| Iconos | Lucide React | — |
| Gráficos | Recharts | — |
| Forms | React Hook Form | — |
| Notificaciones | React Hot Toast | — |
| Estado global | Context API + useReducer | — |

### 3.3 Herramientas de Desarrollo

- Visual Studio 2022 / VS Code
- pgAdmin 4 o DBeaver (gestión de BD)
- Postman / Thunder Client (pruebas de API)
- Git (control de versiones)

---

## 4. Arquitectura del Sistema

### 4.1 Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                     NAVEGADOR DEL CLIENTE                    │
│  ┌────────────────────────────────────────────────────────┐ │
│  │            FRONTEND - React + Tailwind                  │ │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────────────────┐  │ │
│  │  │  Pages   │  │Components│  │  Context (Auth/State)│  │ │
│  │  └──────────┘  └──────────┘  └──────────────────────┘  │ │
│  │  ┌────────────────────────────────────────────────────┐│ │
│  │  │       Services (Axios) - Cliente HTTP              ││ │
│  │  └────────────────────────────────────────────────────┘│ │
│  └────────────────────────────────────────────────────────┘ │
└───────────────────────────────┬─────────────────────────────┘
                                │ HTTPS / JSON
                                │ JWT Token en Header
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                BACKEND - ASP.NET Core Web API                │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Middleware (Auth, CORS, Logging)           │ │
│  └────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────┐ │
│  │                    Controllers (REST)                    │ │
│  │  Auth │ Libros │ Usuarios │ Prestamos │ Reservas │ ... │ │
│  └────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Services (Lógica de Negocio)                │ │
│  └────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────┐ │
│  │           Entity Framework Core (DbContext)              │ │
│  └────────────────────────────────────────────────────────┘ │
└───────────────────────────────┬─────────────────────────────┘
                                │ SQL
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                  POSTGRESQL DATABASE                         │
│  Tablas: Usuarios, Libros, Autores, Categorias,             │
│          Prestamos, Reservas, Multas                        │
└─────────────────────────────────────────────────────────────┘
```

### 4.2 Patrón Arquitectónico

- **Backend:** Arquitectura en capas (Controller → Service → Repository → DbContext)
- **Frontend:** Componentes funcionales con Hooks + Context API
- **Comunicación:** API REST con respuestas JSON, autenticación stateless con JWT

---

## 5. Modelo de Datos

### 5.1 Diagrama Entidad-Relación

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│   Usuario    │         │   Prestamo   │         │    Libro     │
├──────────────┤         ├──────────────┤         ├──────────────┤
│ Id (PK)      │◄────────┤ UsuarioId(FK)│         │ Id (PK)      │
│ Nombre       │   1:N   │ LibroId (FK) ├────────►│ Titulo       │
│ Email        │         │ FechaPrest.  │   N:1   │ ISBN         │
│ PasswordHash │         │ FechaDevEsp. │         │ Año          │
│ Rol          │         │ FechaDevReal │         │ Sinopsis     │
│ FechaReg.    │         │ Estado       │         │ PortadaUrl   │
│ Activo       │         └──────────────┘         │ Stock        │
└──────────────┘                                   │ Disponibles  │
       │                                           │ CategoriaId  │
       │ 1:N                                       │ AutorId      │
       ▼                                           └──────────────┘
┌──────────────┐         ┌──────────────┐                │ N:1
│   Reserva    │         │    Multa     │                │
├──────────────┤         ├──────────────┤      ┌─────────┴─────────┐
│ Id (PK)      │         │ Id (PK)      │      ▼                   ▼
│ UsuarioId    │         │ UsuarioId    │ ┌──────────┐      ┌──────────┐
│ LibroId      │         │ PrestamoId   │ │Categoría │      │  Autor   │
│ FechaReserva │         │ Monto        │ ├──────────┤      ├──────────┤
│ Estado       │         │ Estado       │ │ Id (PK)  │      │ Id (PK)  │
└──────────────┘         │ FechaGen.    │ │ Nombre   │      │ Nombre   │
                         │ FechaPago    │ │ Descrip. │      │Nacionalid│
                         └──────────────┘ └──────────┘      │Biografía │
                                                            └──────────┘
```

### 5.2 Especificación de Tablas

#### Tabla: `usuarios`

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `id` | SERIAL | PK | Identificador único |
| `nombre` | VARCHAR(100) | NOT NULL | Nombre completo |
| `email` | VARCHAR(150) | NOT NULL, UNIQUE | Correo electrónico (login) |
| `password_hash` | VARCHAR(255) | NOT NULL | Contraseña hasheada con BCrypt |
| `rol` | VARCHAR(20) | NOT NULL, DEFAULT 'Lector' | 'Lector' o 'Administrador' |
| `fecha_registro` | TIMESTAMP | NOT NULL, DEFAULT NOW() | Fecha de creación de la cuenta |
| `activo` | BOOLEAN | NOT NULL, DEFAULT true | Estado de la cuenta |
| `telefono` | VARCHAR(20) | NULL | Teléfono opcional |
| `direccion` | VARCHAR(255) | NULL | Dirección opcional |

#### Tabla: `categorias`

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `id` | SERIAL | PK | Identificador único |
| `nombre` | VARCHAR(50) | NOT NULL, UNIQUE | Nombre de la categoría |
| `descripcion` | TEXT | NULL | Descripción de la categoría |

#### Tabla: `autores`

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `id` | SERIAL | PK | Identificador único |
| `nombre` | VARCHAR(100) | NOT NULL | Nombre del autor |
| `nacionalidad` | VARCHAR(50) | NULL | Nacionalidad |
| `biografia` | TEXT | NULL | Biografía breve |
| `fecha_nacimiento` | DATE | NULL | Fecha de nacimiento |

#### Tabla: `libros`

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `id` | SERIAL | PK | Identificador único |
| `titulo` | VARCHAR(200) | NOT NULL | Título del libro |
| `isbn` | VARCHAR(20) | UNIQUE | ISBN del libro |
| `año` | INTEGER | NOT NULL | Año de publicación |
| `editorial` | VARCHAR(100) | NULL | Editorial |
| `sinopsis` | TEXT | NULL | Resumen del libro |
| `portada_url` | VARCHAR(500) | NULL | URL de la imagen de portada |
| `stock` | INTEGER | NOT NULL, DEFAULT 1 | Cantidad total de ejemplares |
| `disponibles` | INTEGER | NOT NULL, DEFAULT 1 | Ejemplares disponibles |
| `categoria_id` | INTEGER | FK → categorias.id | Categoría |
| `autor_id` | INTEGER | FK → autores.id | Autor |
| `fecha_registro` | TIMESTAMP | NOT NULL, DEFAULT NOW() | Fecha de alta |

#### Tabla: `prestamos`

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `id` | SERIAL | PK | Identificador único |
| `usuario_id` | INTEGER | FK → usuarios.id, NOT NULL | Usuario que hizo el préstamo |
| `libro_id` | INTEGER | FK → libros.id, NOT NULL | Libro prestado |
| `fecha_prestamo` | TIMESTAMP | NOT NULL, DEFAULT NOW() | Fecha en que se prestó |
| `fecha_devolucion_esperada` | TIMESTAMP | NOT NULL | Fecha límite de devolución |
| `fecha_devolucion_real` | TIMESTAMP | NULL | Fecha real de devolución |
| `estado` | VARCHAR(20) | NOT NULL, DEFAULT 'Activo' | 'Activo', 'Devuelto', 'Vencido' |
| `observaciones` | TEXT | NULL | Notas adicionales |

#### Tabla: `reservas`

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `id` | SERIAL | PK | Identificador único |
| `usuario_id` | INTEGER | FK → usuarios.id, NOT NULL | Usuario que reservó |
| `libro_id` | INTEGER | FK → libros.id, NOT NULL | Libro reservado |
| `fecha_reserva` | TIMESTAMP | NOT NULL, DEFAULT NOW() | Fecha de la reserva |
| `posicion_cola` | INTEGER | NOT NULL | Posición en la cola |
| `estado` | VARCHAR(20) | NOT NULL, DEFAULT 'Activa' | 'Activa', 'Cumplida', 'Cancelada' |

#### Tabla: `multas`

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `id` | SERIAL | PK | Identificador único |
| `usuario_id` | INTEGER | FK → usuarios.id, NOT NULL | Usuario multado |
| `prestamo_id` | INTEGER | FK → prestamos.id, NOT NULL | Préstamo que generó la multa |
| `monto` | DECIMAL(10,2) | NOT NULL | Monto de la multa |
| `dias_retraso` | INTEGER | NOT NULL | Días de retraso |
| `estado` | VARCHAR(20) | NOT NULL, DEFAULT 'Pendiente' | 'Pendiente', 'Pagada', 'Condonada' |
| `fecha_generacion` | TIMESTAMP | NOT NULL, DEFAULT NOW() | Fecha de la multa |
| `fecha_pago` | TIMESTAMP | NULL | Fecha del pago |

---

## 6. Reglas de Negocio

### 6.1 Préstamos

- **R1.** Un usuario solo puede tener **máximo 3 préstamos activos** simultáneamente.
- **R2.** La duración estándar de un préstamo es de **14 días**.
- **R3.** No se puede prestar un libro si `disponibles = 0`.
- **R4.** Al realizar un préstamo, se decrementa `disponibles` en 1.
- **R5.** Al devolver un libro, se incrementa `disponibles` en 1 y el estado pasa a 'Devuelto'.
- **R6.** Un usuario con multas pendientes **no puede solicitar nuevos préstamos**.
- **R7.** Un usuario inactivo no puede realizar préstamos.

### 6.2 Reservas

- **R8.** Solo se puede reservar un libro si `disponibles = 0`.
- **R9.** Un usuario no puede reservar un libro que ya tiene prestado.
- **R10.** Las reservas siguen un orden FIFO (cola por fecha).
- **R11.** Cuando se devuelve un libro reservado, el primer usuario en la cola tiene 48 horas para retirarlo.
- **R12.** Un usuario puede tener máximo 5 reservas activas.

### 6.3 Multas

- **R13.** Si la fecha actual > `fecha_devolucion_esperada`, el préstamo se marca como 'Vencido'.
- **R14.** Por cada día de retraso se genera una multa de **RD$50.00**.
- **R15.** La multa se calcula automáticamente al devolver el libro o al consultar préstamos vencidos.
- **R16.** Las multas pendientes bloquean nuevos préstamos.

### 6.4 Usuarios y Roles

- **R17.** Existen dos roles: **Lector** (usuario común) y **Administrador**.
- **R18.** Solo administradores pueden crear, editar o eliminar libros, autores y categorías.
- **R19.** Solo administradores acceden al dashboard, reportes y gestión de usuarios.
- **R20.** Un lector solo puede ver y modificar su propia información.

### 6.5 Catálogo

- **R21.** El catálogo es público (no requiere login para ver libros).
- **R22.** Para prestar o reservar sí se requiere autenticación.
- **R23.** La búsqueda funciona por título, autor o ISBN.
- **R24.** Los filtros disponibles son: categoría, autor, año, disponibilidad.

---

## 7. Backend — API REST

### 7.1 Convenciones Generales

- **Base URL:** `https://localhost:5001/api`
- **Formato:** JSON (request y response)
- **Autenticación:** Bearer Token (JWT) en header `Authorization`
- **Códigos HTTP:**
  - `200 OK` — Operación exitosa
  - `201 Created` — Recurso creado
  - `204 No Content` — Operación exitosa sin contenido
  - `400 Bad Request` — Datos inválidos
  - `401 Unauthorized` — No autenticado
  - `403 Forbidden` — Sin permisos
  - `404 Not Found` — Recurso no existe
  - `409 Conflict` — Conflicto (ej: email duplicado)
  - `500 Internal Server Error` — Error del servidor

### 7.2 Endpoints

#### 🔐 Autenticación (`/api/auth`)

| Método | Endpoint | Descripción | Auth | Body |
|---|---|---|---|---|
| POST | `/register` | Registrar usuario nuevo | ❌ | `{ nombre, email, password, telefono?, direccion? }` |
| POST | `/login` | Iniciar sesión | ❌ | `{ email, password }` |
| POST | `/logout` | Cerrar sesión | ✅ | — |
| GET | `/me` | Datos del usuario actual | ✅ | — |

**Respuesta de Login:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "user": {
    "id": 1,
    "nombre": "Juan Pérez",
    "email": "juan@ejemplo.com",
    "rol": "Lector"
  }
}
```

#### 📚 Libros (`/api/libros`)

| Método | Endpoint | Descripción | Auth | Rol |
|---|---|---|---|---|
| GET | `/` | Listar libros con paginación y filtros | ❌ | — |
| GET | `/{id}` | Detalle de un libro | ❌ | — |
| GET | `/buscar?q={query}` | Buscar por título/autor/ISBN | ❌ | — |
| POST | `/` | Crear libro | ✅ | Admin |
| PUT | `/{id}` | Actualizar libro | ✅ | Admin |
| DELETE | `/{id}` | Eliminar libro | ✅ | Admin |

**Query params para listado:**
- `page` (int, default 1)
- `pageSize` (int, default 12)
- `categoriaId` (int, opcional)
- `autorId` (int, opcional)
- `año` (int, opcional)
- `disponibles` (bool, opcional)
- `orden` ('titulo' | 'año' | 'fechaRegistro', default 'titulo')

#### 👥 Usuarios (`/api/usuarios`)

| Método | Endpoint | Descripción | Auth | Rol |
|---|---|---|---|---|
| GET | `/` | Listar todos los usuarios | ✅ | Admin |
| GET | `/{id}` | Detalle de usuario | ✅ | Admin |
| PUT | `/{id}` | Actualizar perfil | ✅ | Owner/Admin |
| PATCH | `/{id}/activar` | Activar/desactivar usuario | ✅ | Admin |
| DELETE | `/{id}` | Eliminar usuario | ✅ | Admin |

#### 📖 Préstamos (`/api/prestamos`)

| Método | Endpoint | Descripción | Auth | Rol |
|---|---|---|---|---|
| GET | `/` | Listar todos los préstamos | ✅ | Admin |
| GET | `/mis-prestamos` | Préstamos del usuario actual | ✅ | — |
| GET | `/{id}` | Detalle de un préstamo | ✅ | Owner/Admin |
| POST | `/` | Solicitar préstamo | ✅ | — |
| PUT | `/{id}/devolver` | Devolver libro | ✅ | — |
| GET | `/vencidos` | Listar préstamos vencidos | ✅ | Admin |

**Body de POST `/prestamos`:**
```json
{
  "libroId": 5
}
```

#### 🔖 Reservas (`/api/reservas`)

| Método | Endpoint | Descripción | Auth | Rol |
|---|---|---|---|---|
| GET | `/mis-reservas` | Reservas del usuario actual | ✅ | — |
| POST | `/` | Crear reserva | ✅ | — |
| DELETE | `/{id}` | Cancelar reserva | ✅ | Owner |
| GET | `/` | Listar todas | ✅ | Admin |

#### 💰 Multas (`/api/multas`)

| Método | Endpoint | Descripción | Auth | Rol |
|---|---|---|---|---|
| GET | `/mis-multas` | Multas del usuario actual | ✅ | — |
| GET | `/` | Listar todas | ✅ | Admin |
| PUT | `/{id}/pagar` | Marcar como pagada | ✅ | Admin |
| PUT | `/{id}/condonar` | Condonar multa | ✅ | Admin |

#### 🏷️ Categorías y Autores

| Método | Endpoint | Descripción | Auth | Rol |
|---|---|---|---|---|
| GET | `/api/categorias` | Listar categorías | ❌ | — |
| POST | `/api/categorias` | Crear categoría | ✅ | Admin |
| PUT | `/api/categorias/{id}` | Editar categoría | ✅ | Admin |
| DELETE | `/api/categorias/{id}` | Eliminar categoría | ✅ | Admin |
| GET | `/api/autores` | Listar autores | ❌ | — |
| POST | `/api/autores` | Crear autor | ✅ | Admin |
| PUT | `/api/autores/{id}` | Editar autor | ✅ | Admin |
| DELETE | `/api/autores/{id}` | Eliminar autor | ✅ | Admin |

#### 📊 Dashboard (`/api/dashboard`)

| Método | Endpoint | Descripción | Auth | Rol |
|---|---|---|---|---|
| GET | `/stats` | Estadísticas generales | ✅ | Admin |
| GET | `/libros-mas-prestados` | Top 10 libros más prestados | ✅ | Admin |
| GET | `/usuarios-activos` | Usuarios más activos | ✅ | Admin |
| GET | `/prestamos-por-mes` | Préstamos agrupados por mes | ✅ | Admin |
| GET | `/categorias-populares` | Categorías más solicitadas | ✅ | Admin |

**Respuesta de `/stats`:**
```json
{
  "totalLibros": 150,
  "totalUsuarios": 45,
  "prestamosActivos": 23,
  "prestamosVencidos": 4,
  "multasPendientes": 6,
  "montoMultasPendientes": 1250.00,
  "reservasActivas": 8
}
```

---

## 8. Frontend — Aplicación React

### 8.1 Páginas y Rutas

| Ruta | Componente | Acceso | Descripción |
|---|---|---|---|
| `/` | `Home` | Público | Página de inicio con libros destacados |
| `/catalogo` | `Catalogo` | Público | Catálogo con búsqueda y filtros |
| `/libro/:id` | `DetalleLibro` | Público | Detalle de un libro específico |
| `/login` | `Login` | Público | Inicio de sesión |
| `/registro` | `Registro` | Público | Registro de nuevo usuario |
| `/perfil` | `Perfil` | Autenticado | Información personal del usuario |
| `/mis-prestamos` | `MisPrestamos` | Autenticado | Préstamos del usuario |
| `/mis-reservas` | `MisReservas` | Autenticado | Reservas del usuario |
| `/mis-multas` | `MisMultas` | Autenticado | Multas del usuario |
| `/admin` | `AdminDashboard` | Admin | Panel de control |
| `/admin/libros` | `GestionLibros` | Admin | CRUD de libros |
| `/admin/usuarios` | `GestionUsuarios` | Admin | Gestión de usuarios |
| `/admin/prestamos` | `GestionPrestamos` | Admin | Todos los préstamos |
| `/admin/multas` | `GestionMultas` | Admin | Gestión de multas |
| `/admin/categorias` | `GestionCategorias` | Admin | CRUD categorías |
| `/admin/autores` | `GestionAutores` | Admin | CRUD autores |
| `/admin/reportes` | `Reportes` | Admin | Reportes y estadísticas |

### 8.2 Componentes Principales

#### Componentes Comunes
- `Navbar` — Barra de navegación superior con menú adaptativo según rol
- `Footer` — Pie de página
- `Sidebar` — Menú lateral (solo en panel admin)
- `Loader` — Indicador de carga
- `Modal` — Ventana modal reutilizable
- `Toast` — Notificaciones temporales
- `ProtectedRoute` — HOC para rutas protegidas
- `Pagination` — Componente de paginación

#### Componentes de Libros
- `BookCard` — Tarjeta de libro en grid
- `BookList` — Lista de libros
- `BookDetail` — Vista detallada
- `BookForm` — Formulario CRUD
- `SearchBar` — Buscador con autocompletado
- `FilterPanel` — Panel de filtros

#### Componentes de Admin
- `DashboardCard` — Tarjeta de métrica
- `StatsChart` — Gráficos con Recharts
- `DataTable` — Tabla con sorting, búsqueda, paginación
- `UserManagement` — Gestión de usuarios

### 8.3 Diseño Visual

#### Paleta de Colores (Tailwind)
- **Primario:** `bg-indigo-600` / `text-indigo-600` (azul intenso)
- **Secundario:** `bg-amber-500` (ámbar para acentos)
- **Fondo:** `bg-gray-50` (claro) / `bg-gray-900` (modo oscuro futuro)
- **Éxito:** `bg-green-500`
- **Error:** `bg-red-500`
- **Advertencia:** `bg-yellow-500`

#### Tipografía
- **Encabezados:** `font-sans font-bold` (Inter o similar)
- **Cuerpo:** `font-sans` (16px base)

#### Layout
- **Responsive:** Mobile-first design
- **Breakpoints:** `sm` (640px), `md` (768px), `lg` (1024px), `xl` (1280px)
- **Grid:** 1 columna en mobile, 2-3 en tablet, 4 en desktop para catálogo

### 8.4 Experiencia de Usuario (UX)

- Feedback visual inmediato en todas las acciones (loading, success, error)
- Confirmaciones antes de operaciones destructivas (eliminar)
- Mensajes de error claros y específicos
- Persistencia de sesión con `localStorage`
- Refresh automático del token JWT
- Navegación intuitiva con breadcrumbs
- Estados vacíos diseñados (sin resultados, sin préstamos, etc.)

---

## 9. Autenticación y Seguridad

### 9.1 Flujo de Autenticación

1. Usuario envía credenciales a `/api/auth/login`
2. Backend valida y genera JWT firmado
3. Frontend guarda el token en `localStorage`
4. Cada request incluye `Authorization: Bearer {token}`
5. Backend valida el token en cada request protegido
6. Si el token expira, redirige a login

### 9.2 Configuración JWT

```json
{
  "Jwt": {
    "Key": "ClaveSuperSecretaDeAlMenos32Caracteres!!",
    "Issuer": "BibliotecaVirtual",
    "Audience": "BibliotecaVirtualUsers",
    "ExpirationHours": 8
  }
}
```

### 9.3 Roles y Permisos

| Recurso | Lector | Administrador |
|---|---|---|
| Ver catálogo | ✅ | ✅ |
| Solicitar préstamo | ✅ | ✅ |
| Hacer reserva | ✅ | ✅ |
| Ver mis préstamos | ✅ | ✅ |
| Editar mi perfil | ✅ | ✅ |
| CRUD libros | ❌ | ✅ |
| CRUD usuarios | ❌ | ✅ |
| Ver todos los préstamos | ❌ | ✅ |
| Gestionar multas | ❌ | ✅ |
| Dashboard | ❌ | ✅ |

### 9.4 Buenas Prácticas de Seguridad

- Contraseñas hasheadas con BCrypt (mínimo 12 rondas)
- Validación en backend de TODA la entrada (nunca confiar en el frontend)
- CORS configurado solo para el dominio del frontend
- Rate limiting en endpoints públicos
- SQL injection prevention vía EF Core (parametrizado por defecto)
- HTTPS obligatorio en producción
- Tokens JWT con expiración corta (8 horas)
- Validación de email único en registro

---

## 10. Estructura de Carpetas

### 10.1 Backend (.NET)

```
BibliotecaAPI/
├── Controllers/
│   ├── AuthController.cs
│   ├── LibrosController.cs
│   ├── UsuariosController.cs
│   ├── PrestamosController.cs
│   ├── ReservasController.cs
│   ├── MultasController.cs
│   ├── CategoriasController.cs
│   ├── AutoresController.cs
│   └── DashboardController.cs
├── Models/
│   ├── Entities/
│   │   ├── Usuario.cs
│   │   ├── Libro.cs
│   │   ├── Autor.cs
│   │   ├── Categoria.cs
│   │   ├── Prestamo.cs
│   │   ├── Reserva.cs
│   │   └── Multa.cs
│   └── Enums/
│       ├── RolUsuario.cs
│       ├── EstadoPrestamo.cs
│       ├── EstadoReserva.cs
│       └── EstadoMulta.cs
├── DTOs/
│   ├── Auth/
│   │   ├── LoginDto.cs
│   │   ├── RegisterDto.cs
│   │   └── AuthResponseDto.cs
│   ├── Libros/
│   │   ├── LibroDto.cs
│   │   ├── CrearLibroDto.cs
│   │   └── ActualizarLibroDto.cs
│   ├── Prestamos/
│   ├── Usuarios/
│   └── Dashboard/
├── Data/
│   ├── BibliotecaContext.cs
│   └── SeedData.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── ILibroService.cs
│   │   ├── IPrestamoService.cs
│   │   ├── IReservaService.cs
│   │   ├── IMultaService.cs
│   │   └── IDashboardService.cs
│   └── Implementations/
│       ├── AuthService.cs
│       ├── LibroService.cs
│       ├── PrestamoService.cs
│       ├── ReservaService.cs
│       ├── MultaService.cs
│       └── DashboardService.cs
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   └── JwtMiddleware.cs
├── Helpers/
│   ├── JwtHelper.cs
│   └── PasswordHelper.cs
├── Validators/
├── Migrations/
├── Properties/
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── BibliotecaAPI.csproj
```

### 10.2 Frontend (React)

```
biblioteca-frontend/
├── public/
│   └── vite.svg
├── src/
│   ├── assets/
│   │   └── images/
│   ├── components/
│   │   ├── common/
│   │   │   ├── Navbar.jsx
│   │   │   ├── Footer.jsx
│   │   │   ├── Sidebar.jsx
│   │   │   ├── Loader.jsx
│   │   │   ├── Modal.jsx
│   │   │   ├── Pagination.jsx
│   │   │   └── ProtectedRoute.jsx
│   │   ├── books/
│   │   │   ├── BookCard.jsx
│   │   │   ├── BookList.jsx
│   │   │   ├── BookDetail.jsx
│   │   │   ├── BookForm.jsx
│   │   │   ├── SearchBar.jsx
│   │   │   └── FilterPanel.jsx
│   │   ├── admin/
│   │   │   ├── DashboardCard.jsx
│   │   │   ├── StatsChart.jsx
│   │   │   └── DataTable.jsx
│   │   └── ui/
│   │       ├── Button.jsx
│   │       ├── Input.jsx
│   │       ├── Select.jsx
│   │       └── Badge.jsx
│   ├── pages/
│   │   ├── Home.jsx
│   │   ├── Catalogo.jsx
│   │   ├── DetalleLibro.jsx
│   │   ├── Login.jsx
│   │   ├── Registro.jsx
│   │   ├── Perfil.jsx
│   │   ├── MisPrestamos.jsx
│   │   ├── MisReservas.jsx
│   │   ├── MisMultas.jsx
│   │   └── admin/
│   │       ├── Dashboard.jsx
│   │       ├── GestionLibros.jsx
│   │       ├── GestionUsuarios.jsx
│   │       ├── GestionPrestamos.jsx
│   │       ├── GestionMultas.jsx
│   │       ├── GestionCategorias.jsx
│   │       ├── GestionAutores.jsx
│   │       └── Reportes.jsx
│   ├── context/
│   │   ├── AuthContext.jsx
│   │   └── ToastContext.jsx
│   ├── services/
│   │   ├── api.js
│   │   ├── authService.js
│   │   ├── libroService.js
│   │   ├── prestamoService.js
│   │   ├── reservaService.js
│   │   ├── multaService.js
│   │   └── dashboardService.js
│   ├── hooks/
│   │   ├── useAuth.js
│   │   ├── useFetch.js
│   │   └── useDebounce.js
│   ├── utils/
│   │   ├── formatters.js
│   │   ├── validators.js
│   │   └── constants.js
│   ├── App.jsx
│   ├── main.jsx
│   └── index.css
├── .env
├── .env.example
├── .gitignore
├── index.html
├── package.json
├── tailwind.config.js
├── postcss.config.js
└── vite.config.js
```

---

## 11. Datos de Prueba (Seed)

El sistema debe inicializar la base de datos con los siguientes datos:

### 11.1 Usuarios

```
1 administrador:
  - Email: admin@biblioteca.com
  - Password: Admin123!
  - Nombre: Administrador del Sistema
  - Rol: Administrador

3 lectores:
  - lector1@test.com / Lector123! - Juan Pérez
  - lector2@test.com / Lector123! - María García
  - lector3@test.com / Lector123! - Carlos Rodríguez
```

### 11.2 Categorías (10)

Ficción, No Ficción, Ciencia, Historia, Tecnología, Literatura Clásica, Biografía, Autoayuda, Infantil, Académico.

### 11.3 Autores (15)

Gabriel García Márquez, Mario Vargas Llosa, Isabel Allende, Julio Cortázar, Jorge Luis Borges, Pablo Neruda, Octavio Paz, Juan Bosch, Manuel del Cabral, Pedro Mir, Stephen Hawking, Yuval Harari, Carl Sagan, Robert Martin, Andrew Hunt.

### 11.4 Libros (20+)

Variedad de libros que cubran todas las categorías, con datos realistas:
- Título, ISBN único, año, editorial
- Sinopsis descriptiva (2-3 párrafos)
- URL de portada (puede usar Unsplash, Open Library Covers, o placeholder)
- Stock entre 1 y 5 ejemplares

### 11.5 Préstamos y Reservas

- 5 préstamos activos
- 2 préstamos vencidos (para probar multas)
- 3 préstamos devueltos
- 2 reservas activas

---

## 12. Instrucciones de Instalación

### 12.1 Requisitos Previos

- .NET 10 SDK
- Node.js 18+
- PostgreSQL 14+
- Git

### 12.2 Configuración del Backend

```bash
# Clonar el repositorio
git clone <repo-url>
cd BibliotecaAPI

# Restaurar dependencias
dotnet restore

# Configurar cadena de conexión en appsettings.json
# "ConnectionStrings": {
#   "Default": "Host=localhost;Port=5432;Database=biblioteca_virtual;Username=postgres;Password=tu_password"
# }

# Crear migración inicial
dotnet ef migrations add InitialCreate

# Aplicar migraciones (crea la BD)
dotnet ef database update

# Ejecutar el proyecto
dotnet run

# La API estará disponible en https://localhost:5001
# Swagger en https://localhost:5001/swagger
```

### 12.3 Configuración del Frontend

```bash
# Ir al directorio del frontend
cd biblioteca-frontend

# Instalar dependencias
npm install

# Crear archivo .env
echo "VITE_API_URL=https://localhost:5001/api" > .env

# Ejecutar en modo desarrollo
npm run dev

# La app estará disponible en http://localhost:5173
```

### 12.4 Variables de Entorno

**Backend (`appsettings.json`):**
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=biblioteca_virtual;Username=postgres;Password=admin"
  },
  "Jwt": {
    "Key": "ClaveSuperSecretaDeAlMenos32Caracteres!!",
    "Issuer": "BibliotecaVirtual",
    "Audience": "BibliotecaVirtualUsers",
    "ExpirationHours": 8
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

**Frontend (`.env`):**
```env
VITE_API_URL=https://localhost:5001/api
VITE_APP_NAME=Biblioteca Virtual UNICARIBE
```

---

## 13. Criterios de Aceptación

### 13.1 Funcionales

- ✅ Un usuario puede registrarse con email único
- ✅ Un usuario puede iniciar sesión y recibir un JWT válido
- ✅ Un usuario autenticado puede ver el catálogo completo
- ✅ Un usuario puede buscar libros por título, autor o ISBN
- ✅ Un usuario puede filtrar libros por categoría, autor y año
- ✅ Un usuario puede solicitar un préstamo si tiene menos de 3 activos y sin multas
- ✅ Un usuario puede devolver un libro y se actualiza el stock
- ✅ Si un préstamo se vence, se genera una multa automáticamente
- ✅ Un usuario puede reservar un libro no disponible
- ✅ Un administrador puede crear, editar y eliminar libros
- ✅ Un administrador puede gestionar usuarios (activar/desactivar)
- ✅ El dashboard muestra estadísticas en tiempo real
- ✅ Los gráficos del dashboard reflejan datos reales de la BD

### 13.2 No Funcionales

- ✅ La aplicación es totalmente responsive (mobile, tablet, desktop)
- ✅ Las páginas cargan en menos de 2 segundos
- ✅ El API responde en menos de 500ms para operaciones simples
- ✅ La autenticación funciona correctamente con JWT
- ✅ Los datos persisten correctamente en PostgreSQL
- ✅ El código está comentado en español
- ✅ No hay errores en la consola del navegador
- ✅ El código sigue convenciones de naming (PascalCase en C#, camelCase en JS)

### 13.3 De Seguridad

- ✅ Las contraseñas se almacenan hasheadas (BCrypt)
- ✅ Los endpoints protegidos rechazan requests sin JWT válido
- ✅ Los endpoints de admin rechazan requests de usuarios no admin
- ✅ Las inyecciones SQL están prevenidas por EF Core
- ✅ CORS configurado correctamente

---

## 14. Entregables

### 14.1 Código Fuente

1. **Repositorio Git** con la siguiente estructura:
   - `/backend` — API en .NET
   - `/frontend` — Aplicación React
   - `/docs` — Documentación adicional
   - `README.md` — Instrucciones de instalación y uso

### 14.2 Documentación

1. **README.md** principal con:
   - Descripción del proyecto
   - Tecnologías utilizadas
   - Instrucciones de instalación
   - Capturas de pantalla
   - Credenciales de prueba
   - Diagrama de arquitectura

2. **Documentación API** generada por Swagger

3. **Script SQL** de respaldo de la base de datos con datos seed

### 14.3 Material para la Exposición

1. Presentación en PowerPoint o similar
2. Demo en vivo funcional
3. Documento APA con marco teórico completo

---

## 📌 Notas Finales para la IA Desarrolladora

### Instrucciones específicas

1. **Idioma:** Todo el código, comentarios, variables y documentación deben estar en **español latinoamericano (cero idioma argentino)**.
2. **Comentar el código:** Cada función importante debe tener comentarios explicando qué hace, especialmente para que un estudiante de primer taller pueda defenderlo.
3. **Manejo de errores:** Implementar try/catch en todos los controladores y servicios, con respuestas estructuradas.
4. **Validaciones:** Implementar validaciones tanto en frontend como en backend.
5. **Responsive:** Cada componente del frontend DEBE ser responsive desde el inicio.
6. **Diseño moderno:** Usar elementos visuales modernos: cards con sombras suaves, transiciones, hover effects, gradientes sutiles.
7. **Accesibilidad:** Usar etiquetas semánticas HTML5, labels en formularios, aria-labels donde corresponda.
8. **Performance:** Implementar lazy loading de rutas, paginación en listas largas, debounce en búsquedas.
9. **Estructura limpia:** Separar responsabilidades, no mezclar lógica en componentes UI.
10. **Datos de prueba:** SIEMPRE incluir seed data realista para que el sistema pueda demostrarse sin configuración manual.

### Orden de implementación sugerido

1. **Fase 1:** Backend — Setup, modelos, DbContext, migraciones, seed
2. **Fase 2:** Backend — AuthController + JWT
3. **Fase 3:** Backend — Controllers de Libros, Categorías, Autores
4. **Fase 4:** Backend — Controllers de Préstamos, Reservas, Multas (con reglas)
5. **Fase 5:** Backend — DashboardController
6. **Fase 6:** Frontend — Setup Vite + Tailwind + Router
7. **Fase 7:** Frontend — AuthContext + Login/Register
8. **Fase 8:** Frontend — Home + Catálogo + Detalle
9. **Fase 9:** Frontend — Perfil + Mis Préstamos/Reservas/Multas
10. **Fase 10:** Frontend — Panel Admin (Dashboard + CRUDs)
11. **Fase 11:** Pulir UI, testing manual, fixing bugs
12. **Fase 12:** Documentación y README final

---

**Versión del PRD:** 1.0
**Fecha:** Mayo 2026
**Autor:** Grupo 1 — Taller de Programación I — UNICARIBE
