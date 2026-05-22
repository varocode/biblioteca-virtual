import { api } from './api.js';

export async function fetchLoans() {
  const { data } = await api.get('/prestamos');
  return data;
}

export async function createLoan(libroId) {
  const { data } = await api.post('/prestamos', { libroId: Number(libroId) });
  return data;
}

export async function returnLoan(id) {
  const { data } = await api.post(`/prestamos/${id}/devolver`);
  return data;
}

export async function approveLoan(id) {
  const { data } = await api.post(`/prestamos/${id}/aprobar`);
  return data;
}

export async function renewLoan(id) {
  const { data } = await api.post(`/prestamos/${id}/renovar`);
  return data;
}

export async function fetchReservations() {
  const { data } = await api.get('/reservas');
  return data;
}

export async function createReservation(libroId) {
  const { data } = await api.post('/reservas', { libroId: Number(libroId) });
  return data;
}

export async function cancelReservation(id) {
  await api.delete(`/reservas/${id}`);
}

export async function prepareReservationPickup(id) {
  const { data } = await api.post(`/reservas/${id}/preparar-retiro`);
  return data;
}

export async function fetchFines() {
  const { data } = await api.get('/multas');
  return data;
}

export async function payFine(id, aprobar = true) {
  const { data } = await api.post(`/multas/${id}/pagar`, { aprobar });
  return data;
}

export async function fetchNotifications() {
  const { data } = await api.get('/notificaciones');
  return data;
}
