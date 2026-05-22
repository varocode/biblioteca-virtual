import { api } from './api.js';

export async function fetchBooks(params = {}) {
  const { data } = await api.get('/libros', { params: cleanParams(params) });
  return data;
}

export async function fetchBook(id) {
  const { data } = await api.get(`/libros/${id}`);
  return data;
}

export async function fetchAuthors() {
  const { data } = await api.get('/autores');
  return data;
}

export async function fetchCategories() {
  const { data } = await api.get('/categorias');
  return data;
}

function cleanParams(params) {
  return Object.fromEntries(
    Object.entries(params).filter(([, value]) => value !== '' && value !== null && value !== undefined)
  );
}
