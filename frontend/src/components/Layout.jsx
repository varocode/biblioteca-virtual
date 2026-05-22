import { Link, NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';

const navClass = ({ isActive }) =>
  `rounded-full px-4 py-2 text-sm font-semibold transition ${
    isActive
      ? 'bg-lavender-500 text-white shadow-glow'
      : 'text-ink-700 hover:bg-lavender-100 hover:text-lavender-700'
  }`;

export default function Layout() {
  const { isAuthenticated, isAdmin, user, logout } = useAuth();

  return (
    <div className="min-h-screen">
      <header className="sticky top-0 z-30 border-b border-white/40 bg-white/75 backdrop-blur-xl">
        <div className="mx-auto max-w-6xl px-4 py-4">
          <div className="flex flex-wrap items-center justify-between gap-4">
            <Link to="/catalogo" className="group flex items-center gap-4">
              <span className="grid h-16 w-16 place-items-center rounded-2xl bg-gradient-to-br from-lavender-100 via-white to-peach-100 p-2 shadow-soft ring-2 ring-white">
                <img src="/unicaribe-logo.png" alt="Unicaribe" className="h-full w-full object-contain" />
              </span>
              <span className="flex flex-col leading-tight">
                <span className="text-xs font-bold uppercase tracking-[0.22em] text-lavender-700">
                  Unicaribe
                </span>
                <span className="font-display text-2xl font-extrabold text-ink-900 group-hover:text-lavender-700">
                  Biblioteca Virtual
                </span>
              </span>
            </Link>

            <div className="flex items-center gap-3">
              {!isAuthenticated ? (
                <>
                  <NavLink
                    to="/login"
                    className="rounded-full border border-lavender-200 bg-white px-4 py-2 text-sm font-semibold text-ink-900 transition hover:border-lavender-400 hover:bg-lavender-50 hover:text-lavender-700"
                  >
                    Ingresar
                  </NavLink>
                  <NavLink
                    to="/registro"
                    className="rounded-full bg-gradient-to-r from-lavender-500 to-peach-500 px-4 py-2 text-sm font-semibold text-white shadow-glow transition hover:opacity-95"
                  >
                    Crear cuenta
                  </NavLink>
                </>
              ) : (
                <>
                  <span className="hidden text-sm text-ink-700 sm:inline">
                    Hola, <span className="font-semibold text-ink-900">{user?.nombre || 'lector/a'}</span>
                  </span>
                  <button
                    onClick={logout}
                    className="rounded-full border border-lavender-200 bg-white px-4 py-2 text-sm font-semibold text-ink-900 transition hover:border-peach-300 hover:bg-peach-100 hover:text-peach-700"
                  >
                    Salir
                  </button>
                </>
              )}
            </div>
          </div>

          <nav className="mt-4 flex flex-wrap items-center gap-1.5">
            <NavLink to="/catalogo" className={navClass}>Catálogo</NavLink>
            {isAuthenticated && <NavLink to="/mi-biblioteca" className={navClass}>Mi biblioteca</NavLink>}
            {isAuthenticated && <NavLink to="/prestamos" className={navClass}>Préstamos</NavLink>}
            {isAuthenticated && <NavLink to="/reservas" className={navClass}>Reservas</NavLink>}
            {isAuthenticated && <NavLink to="/multas" className={navClass}>Multas</NavLink>}
            {isAuthenticated && <NavLink to="/notificaciones" className={navClass}>Notificaciones</NavLink>}
            {isAuthenticated && <NavLink to="/perfil" className={navClass}>Mi perfil</NavLink>}
            {isAdmin && <NavLink to="/admin" className={navClass}>Admin</NavLink>}
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-4 py-10">
        <Outlet />
      </main>

      <footer className="mx-auto mt-12 max-w-6xl px-4 pb-8 text-center text-xs text-ink-500">
        <p className="font-semibold uppercase tracking-[0.22em] text-lavender-700">Universidad del Caribe</p>
        <p className="mt-1">
          Trabajo final del <strong className="text-ink-900">Taller de Programación I</strong> · Biblioteca Virtual
        </p>
      </footer>
    </div>
  );
}
