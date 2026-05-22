export function formatDate(value) {
  if (!value) return '—';
  return new Intl.DateTimeFormat('es-DO', { dateStyle: 'medium' }).format(new Date(value));
}

export function formatMoney(value) {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(Number(value ?? 0));
}

export function enumLabel(value, labels) {
  return labels[value] ?? value ?? '—';
}

export const loanStatus = { 0: 'Pendiente', 1: 'Activo', 2: 'Devuelto', 3: 'Vencido', Pendiente: 'Pendiente', Activo: 'Activo', Devuelto: 'Devuelto', Vencido: 'Vencido' };
export const reservationStatus = { 1: 'Activa', 2: 'Asignada', 3: 'Cumplida', 4: 'Cancelada', 5: 'Expirada', Activa: 'Activa', Asignada: 'Asignada', Cumplida: 'Cumplida', Cancelada: 'Cancelada', Expirada: 'Expirada' };
export const fineStatus = { 1: 'Pendiente', 2: 'Pagada', 3: 'Condonada', Pendiente: 'Pendiente', Pagada: 'Pagada', Condonada: 'Condonada' };
export const roleLabel = { 1: 'Lector', 2: 'Administrador', Lector: 'Lector', Administrador: 'Administrador' };
