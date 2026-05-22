import { useState } from 'react';
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import StatusMessage from '../components/StatusMessage.jsx';
import { useAuth } from '../context/AuthContext.jsx';
import { getErrorMessage } from '../services/api.js';

export default function LoginPage() {
  const { login, loading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [params] = useSearchParams();
  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState('');

  async function onSubmit(event) {
    event.preventDefault();
    setError('');
    try {
      await login(form);
      navigate(location.state?.from?.pathname ?? '/catalogo', { replace: true });
    } catch (err) {
      setError(getErrorMessage(err, 'No pudimos iniciar sesión.'));
    }
  }

  return (
    <section className="mx-auto max-w-5xl overflow-hidden rounded-[2rem] border border-white/70 bg-white/85 shadow-soft backdrop-blur-sm">
      <div className="grid md:grid-cols-[1.1fr_1fr]">
        <aside className="relative hidden flex-col justify-between bg-pastel-hero p-10 md:flex">
          <div className="flex items-center gap-3">
            <span className="grid h-14 w-14 place-items-center rounded-2xl bg-white shadow-soft">
              <img src="/unicaribe-logo.png" alt="Unicaribe" className="h-10 w-10 object-contain" />
            </span>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-lavender-700">Unicaribe</p>
              <p className="font-display text-lg font-bold text-ink-900">Biblioteca Virtual</p>
            </div>
          </div>

          <div>
            <h2 className="font-display text-3xl font-extrabold leading-tight text-ink-900">
              Tu biblioteca,<br />a un clic.
            </h2>
            <p className="mt-3 text-sm text-ink-700/80">
              Reserva títulos, gestiona tus préstamos y descubre lecturas nuevas pensadas para la
              comunidad de la Universidad del Caribe.
            </p>
          </div>

          <ul className="space-y-2 text-sm text-ink-700">
            <li className="flex items-center gap-2"><span className="h-1.5 w-1.5 rounded-full bg-lavender-500" /> Catálogo en tiempo real</li>
            <li className="flex items-center gap-2"><span className="h-1.5 w-1.5 rounded-full bg-peach-500" /> Reservas y préstamos digitales</li>
            <li className="flex items-center gap-2"><span className="h-1.5 w-1.5 rounded-full bg-mint-500" /> Notificaciones personalizadas</li>
          </ul>
        </aside>

        <div className="p-8 md:p-10">
          <div className="md:hidden mb-6 flex items-center gap-3">
            <span className="grid h-12 w-12 place-items-center rounded-2xl bg-white shadow-soft ring-1 ring-lavender-100">
              <img src="/unicaribe-logo.png" alt="Unicaribe" className="h-9 w-9 object-contain" />
            </span>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-lavender-700">Unicaribe</p>
              <p className="font-display text-base font-bold text-ink-900">Biblioteca Virtual</p>
            </div>
          </div>

          <h1 className="font-display text-3xl font-extrabold text-ink-900">¡Bienvenida/o de nuevo!</h1>
          <p className="mt-2 text-sm text-ink-500">Ingresa con tu cuenta institucional para continuar.</p>

          <div className="mt-6 space-y-4">
            {params.get('session') === 'expired' && (
              <StatusMessage type="error">Tu sesión expiró. Inicia sesión de nuevo.</StatusMessage>
            )}
            {error && <StatusMessage type="error">{error}</StatusMessage>}
          </div>

          <form className="mt-4 space-y-4" onSubmit={onSubmit}>
            <label className="block text-sm font-semibold text-ink-700">
              Email
              <input
                required
                type="email"
                className="input-pastel mt-1 w-full"
                placeholder="tu@unicaribe.edu"
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
              />
            </label>
            <label className="block text-sm font-semibold text-ink-700">
              Contraseña
              <input
                required
                type="password"
                className="input-pastel mt-1 w-full"
                placeholder="••••••••"
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
              />
            </label>
            <button
              disabled={loading}
              className="w-full rounded-full bg-gradient-to-r from-lavender-500 to-peach-500 px-4 py-2.5 font-semibold text-white shadow-glow transition hover:opacity-95 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {loading ? 'Ingresando...' : 'Ingresar'}
            </button>
          </form>

          <p className="mt-6 text-sm text-ink-500">
            ¿No tienes cuenta?{' '}
            <Link className="font-semibold text-lavender-700 hover:text-lavender-500" to="/registro">
              Regístrate
            </Link>
          </p>
        </div>
      </div>
    </section>
  );
}
