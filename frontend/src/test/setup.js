import '@testing-library/jest-dom/vitest';
import { afterEach } from 'vitest';
import { cleanup } from '@testing-library/react';

const storage = new Map();
const localStorageMock = {
  getItem: (key) => storage.get(key) ?? null,
  setItem: (key, value) => storage.set(key, String(value)),
  removeItem: (key) => storage.delete(key),
  clear: () => storage.clear()
};

Object.defineProperty(globalThis, 'localStorage', {
  value: localStorageMock,
  configurable: true
});

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
  configurable: true
});

afterEach(() => {
  cleanup();
  localStorage.clear();
});
