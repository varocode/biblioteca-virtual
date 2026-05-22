import { api } from './api.js';

export async function fetchUsers() {
  const { data } = await api.get('/usuarios');
  return data;
}

export async function updateUser(id, payload) {
  const { data } = await api.put(`/usuarios/${id}`, payload);
  return data;
}

export async function fetchDashboard() {
  const { data } = await api.get('/dashboard/resumen');
  return data;
}

export async function fetchCopies(params = {}) {
  const { data } = await api.get('/ejemplares', { params });
  return data;
}

export async function fetchAudit(params = {}) {
  const { data } = await api.get('/audit', { params });
  return data;
}

export async function saveBook(payload, id) {
  const body = normalizeBook(payload);
  const { data } = id ? await api.put(`/libros/${id}`, body) : await api.post('/libros', body);
  return data;
}

export async function deleteBook(id) {
  await api.delete(`/libros/${id}`);
}

export async function saveAuthor(payload, id) {
  const { data } = id ? await api.put(`/autores/${id}`, payload) : await api.post('/autores', payload);
  return data;
}

export async function deleteAuthor(id) {
  await api.delete(`/autores/${id}`);
}

export async function saveCategory(payload, id) {
  const { data } = id ? await api.put(`/categorias/${id}`, payload) : await api.post('/categorias', payload);
  return data;
}

export async function deleteCategory(id) {
  await api.delete(`/categorias/${id}`);
}

function normalizeBook(book) {
  return {
    ...book,
    anio: Number(book.anio),
    stock: Number(book.stock),
    disponibles: Number(book.disponibles),
    autorId: Number(book.autorId),
    categoriaId: Number(book.categoriaId)
  };
}
