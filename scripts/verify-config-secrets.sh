#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

FILES=(
  "$ROOT_DIR/.env.example"
  "$ROOT_DIR/backend/.env.example"
  "$ROOT_DIR/frontend/.env.example"
  "$ROOT_DIR/backend/appsettings.json"
  "$ROOT_DIR/README.md"
  "$ROOT_DIR/docs/demo.md"
  "$ROOT_DIR/docs/api.md"
)

python3 - "${FILES[@]}" <<'PY'
import re
import sys
from pathlib import Path

allowed_demo_values = {
    "Admin123!",
    "Lector123!",
    "admin@biblioteca.com",
    "lector1@test.com",
    "lector2@test.com",
    "lector3@test.com",
    "change-me-local-only",
    "replace-with-a-local-demo-key-at-least-32-chars",
    "CAMBIAR_PASSWORD",
    "CAMBIAR_POR_UNA_CLAVE_LOCAL_DE_32_CARACTERES",
}

placeholder_markers = (
    "change-me",
    "replace-with",
    "CAMBIAR",
    "placeholder",
    "local-demo",
    "local-only",
)

secret_key_re = re.compile(r"(?i)(password|passwd|pwd|secret|token|jwt__key|jwt\W*key|api[_-]?key|private[_-]?key)")
assignment_re = re.compile(r"^\s*([A-Za-z0-9_:.\-]+)\s*[=:]\s*[\"']?([^\"'#\n]+)")
private_key_re = re.compile(r"-----BEGIN [A-Z ]*PRIVATE KEY-----")

failures: list[str] = []

for raw_path in sys.argv[1:]:
    path = Path(raw_path)
    if not path.exists():
        failures.append(f"missing expected scan file: {path}")
        continue
    for line_no, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
        if private_key_re.search(line):
            failures.append(f"{path}:{line_no}: private key material is not allowed")
            continue

        match = assignment_re.match(line)
        if not match:
            continue

        key, value = match.group(1), match.group(2).strip().rstrip(",")
        value = value.strip(' \"\'')

        if not secret_key_re.search(key):
            continue
        if not value or value in allowed_demo_values or any(marker in value for marker in placeholder_markers):
            continue

        # Connection strings are allowed only when their embedded password is a placeholder.
        if "Password=" in value:
            pwd = value.split("Password=", 1)[1].split(";", 1)[0]
            if pwd in allowed_demo_values or any(marker in pwd for marker in placeholder_markers):
                continue

        failures.append(f"{path}:{line_no}: {key} must use a placeholder/local demo value, not `{value}`")

if failures:
    print("Secret/config scan failed:")
    for failure in failures:
        print(f"- {failure}")
    sys.exit(1)

print(f"Secret/config scan passed for {len(sys.argv) - 1} files. Only placeholders or documented demo credentials were found.")
PY
