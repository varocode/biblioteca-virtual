import { Link, Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { normalizeRole } from '../utils/roles.js';

export default function ProtectedRoute({ roles }) {
  const { isAuthenticated, isAdmin, user } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (roles?.length) {
    const role = normalizeRole(user?.rol);
    if (!roles.includes(role) && !(roles.includes('Administrador') && isAdmin)) {
      return <UnauthorizedMessage />;
    }
  }

  return <Outlet />;
}

function UnauthorizedMessage() {
  return (
    <section className="mx-auto max-w-2xl rounded-3xl border border-peach-200 bg-gradient-to-br from-peach-100 via-white to-cream-50 p-8 shadow-soft">
      <p className="inline-flex items-center gap-2 rounded-full bg-peach-200 px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] text-peach-700">
        <span className="h-1.5 w-1.5 rounded-full bg-peach-500" />
        Acceso restringido
      </p>
      <h1 className="mt-3 font-display text-3xl font-extrabold text-ink-900">
        No tienes permisos para entrar a esta sección.
      </h1>
      <p className="mt-2 text-sm text-ink-700">
        Tu sesión está activa, pero esta página es solo para administradores.
      </p>
      <Link
        className="mt-5 inline-flex rounded-full bg-gradient-to-r from-lavender-500 to-peach-500 px-5 py-2 text-sm font-semibold text-white shadow-glow transition hover:opacity-95"
        to="/catalogo"
      >
        Volver al catálogo
      </Link>
    </section>
  );
}
