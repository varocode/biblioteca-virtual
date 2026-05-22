import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ProtectedRoute from './ProtectedRoute.jsx';

const authState = vi.hoisted(() => ({ value: { isAuthenticated: false, isAdmin: false, user: null } }));

vi.mock('../context/AuthContext.jsx', () => ({
  useAuth: () => authState.value
}));

function renderGuard() {
  render(
    <MemoryRouter initialEntries={["/admin"]}>
      <Routes>
        <Route element={<ProtectedRoute roles={["Administrador"]} />}>
          <Route path="/admin" element={<p>Admin privado</p>} />
        </Route>
        <Route path="/login" element={<p>Login requerido</p>} />
        <Route path="/catalogo" element={<p>Catálogo público</p>} />
      </Routes>
    </MemoryRouter>
  );
}

describe('ProtectedRoute', () => {
  beforeEach(() => {
    authState.value = { isAuthenticated: false, isAdmin: false, user: null };
  });

  it('redirects anonymous users to login', () => {
    renderGuard();
    expect(screen.getByText('Login requerido')).toBeInTheDocument();
  });

  it('shows a friendly unauthorized message for non-admin readers', () => {
    authState.value = { isAuthenticated: true, isAdmin: false, user: { rol: 'Lector' } };
    renderGuard();
    expect(screen.getByText('No tienes permisos para entrar a esta sección.')).toBeInTheDocument();
  });

  it('does not treat numeric reader role as admin', () => {
    authState.value = { isAuthenticated: true, isAdmin: false, user: { rol: 1 } };
    renderGuard();
    expect(screen.getByText('No tienes permisos para entrar a esta sección.')).toBeInTheDocument();
  });

  it('allows admins to enter admin pages', () => {
    authState.value = { isAuthenticated: true, isAdmin: true, user: { rol: 'Administrador' } };
    renderGuard();
    expect(screen.getByText('Admin privado')).toBeInTheDocument();
  });
});
