#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BACKEND_URL="${BACKEND_URL:-http://127.0.0.1:5000}"
FRONTEND_URL="${FRONTEND_URL:-http://127.0.0.1:4173}"
PG_PORT="${PG_PORT:-54329}"
CONNECTION_STRING="${ConnectionStrings__DefaultConnection:-Host=127.0.0.1;Port=${PG_PORT};Database=biblioteca_virtual;Username=postgres;Password=change-me-local-only}"
START_POSTGRES="${START_POSTGRES:-auto}"
SKIP_INSTALL="${SKIP_INSTALL:-0}"

BACKEND_LOG="$(mktemp)"
FRONTEND_LOG="$(mktemp)"
BACKEND_PID=""
FRONTEND_PID=""

cleanup() {
  if [[ -n "$FRONTEND_PID" ]]; then kill "$FRONTEND_PID" >/dev/null 2>&1 || true; fi
  if [[ -n "$BACKEND_PID" ]]; then kill "$BACKEND_PID" >/dev/null 2>&1 || true; fi
  rm -f "$BACKEND_LOG" "$FRONTEND_LOG"
}
trap cleanup EXIT

wait_for_url() {
  local url="$1"
  local label="$2"
  local attempts="${3:-60}"
  for _ in $(seq 1 "$attempts"); do
    if curl --fail --silent --show-error --max-time 3 "$url" >/dev/null; then
      echo "ok: $label -> $url"
      return 0
    fi
    sleep 2
  done
  echo "error: timed out waiting for $label -> $url" >&2
  return 1
}

if [[ "$START_POSTGRES" == "1" || "$START_POSTGRES" == "auto" ]]; then
  if command -v docker >/dev/null 2>&1; then
    echo "Starting PostgreSQL with docker compose on port ${PG_PORT}..."
    if ! docker compose -f "$ROOT_DIR/docker-compose.yml" up -d postgres; then
      if [[ "$START_POSTGRES" == "1" ]]; then
        echo "error: Docker is installed but PostgreSQL could not be started." >&2
        exit 1
      fi
      echo "Docker is unavailable; assuming PostgreSQL is already available via ConnectionStrings__DefaultConnection."
    fi
  elif [[ "$START_POSTGRES" == "1" ]]; then
    echo "error: START_POSTGRES=1 requires docker" >&2
    exit 1
  else
    echo "Docker not found; assuming PostgreSQL is already available via ConnectionStrings__DefaultConnection."
  fi
fi

if [[ "$SKIP_INSTALL" != "1" ]]; then
  dotnet restore "$ROOT_DIR/backend/BibliotecaAPI.csproj"
  if [[ -f "$ROOT_DIR/frontend/package-lock.json" ]]; then
    npm --prefix "$ROOT_DIR/frontend" ci
  else
    npm --prefix "$ROOT_DIR/frontend" install
  fi
fi

echo "Starting backend..."
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS="$BACKEND_URL" \
ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
Jwt__Key="local-demo-key-for-startup-verification-32-chars" \
Jwt__Issuer="BibliotecaVirtual" \
Jwt__Audience="BibliotecaVirtualUsers" \
Cors__AllowedOrigins__0="$FRONTEND_URL" \
dotnet run --project "$ROOT_DIR/backend/BibliotecaAPI.csproj" --no-restore >"$BACKEND_LOG" 2>&1 &
BACKEND_PID=$!

wait_for_url "$BACKEND_URL/health" "backend health"
wait_for_url "$BACKEND_URL/swagger/index.html" "Swagger UI"

echo "Checking seeded login and catalog data..."
LOGIN_RESPONSE="$(curl --fail --silent --show-error \
  -H 'Content-Type: application/json' \
  -d '{"email":"admin@biblioteca.com","password":"Admin123!"}' \
  "$BACKEND_URL/api/auth/login")"

CATALOG_RESPONSE="$(curl --fail --silent --show-error "$BACKEND_URL/api/libros?page=1&pageSize=5")"

python3 - "$LOGIN_RESPONSE" "$CATALOG_RESPONSE" <<'PY'
import json
import sys

login = json.loads(sys.argv[1])
catalog = json.loads(sys.argv[2])

if not login.get("token"):
    raise SystemExit("login response did not include a JWT token")
user = login.get("usuario") or {}
if user.get("email") != "admin@biblioteca.com" or user.get("rol") != "Administrador":
    raise SystemExit("seeded admin credentials did not return the expected admin user")
if catalog.get("total", 0) < 20 or len(catalog.get("items", [])) == 0:
    raise SystemExit("catalog seed data was not available through /api/libros")
print("ok: seeded admin credentials and catalog data are available")
PY

echo "Building frontend..."
npm --prefix "$ROOT_DIR/frontend" run build

echo "Starting frontend preview..."
VITE_API_BASE_URL="$BACKEND_URL/api" npm --prefix "$ROOT_DIR/frontend" run preview -- --host 127.0.0.1 --port "${FRONTEND_URL##*:}" >"$FRONTEND_LOG" 2>&1 &
FRONTEND_PID=$!

wait_for_url "$FRONTEND_URL" "frontend preview"

echo "Demo startup verification passed: backend health, Swagger, frontend build/preview, and seeded demo credentials/data are reproducible."
