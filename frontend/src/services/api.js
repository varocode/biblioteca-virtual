import axios from 'axios';

const TOKEN_KEY = 'biblioteca.auth';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:5001/api',
  headers: { 'Content-Type': 'application/json' }
});

let sessionExpiredHandler = null;

export function setSessionExpiredHandler(handler) {
  sessionExpiredHandler = handler;
}

export function getStoredAuth() {
  try {
    const raw = localStorage.getItem(TOKEN_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    localStorage.removeItem(TOKEN_KEY);
    return null;
  }
}

export function storeAuth(auth) {
  localStorage.setItem(TOKEN_KEY, JSON.stringify(auth));
}

export function clearStoredAuth() {
  localStorage.removeItem(TOKEN_KEY);
}

api.interceptors.request.use((config) => {
  const auth = getStoredAuth();
  if (auth?.token) {
    config.headers.Authorization = `Bearer ${auth.token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      clearStoredAuth();
      sessionExpiredHandler?.();
    }
    return Promise.reject(error);
  }
);

export function getErrorMessage(error, fallback = 'Ocurrió un error inesperado.') {
  const status = error?.response?.status;
  if (error?.response?.data?.mensaje) return error.response.data.mensaje;
  if (status === 401) return 'Tu sesión expiró o no está autorizada. Inicia sesión de nuevo.';
  if (status === 403) return 'No tienes permisos para realizar esta acción.';
  if (status === 404) return 'No encontramos el recurso solicitado.';
  if (status >= 500) return 'El servidor tuvo un problema. Intentá nuevamente en unos minutos.';
  if (error?.code === 'ERR_NETWORK' || error?.message === 'Network Error' || !error?.response) {
    return 'No pudimos conectar con el servidor. Verificá que el backend esté iniciado y que la URL del API sea correcta.';
  }
  return fallback;
}
