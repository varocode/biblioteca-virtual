export function normalizeRole(role) {
  if (role === 'Administrador' || role === 2) return 'Administrador';
  if (role === 'Lector' || role === 1) return 'Lector';
  return role ?? '';
}

export function isAdminRole(role) {
  return normalizeRole(role) === 'Administrador';
}
