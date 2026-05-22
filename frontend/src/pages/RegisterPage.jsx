import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import StatusMessage from '../components/StatusMessage.jsx';
import { useAuth } from '../context/AuthContext.jsx';
import { getErrorMessage } from '../services/api.js';

export default function RegisterPage() {
  const { register, loading } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ nombre: '', email: '', password: '', telefono: '', direccion: '' });
  const [error, setError] = useState('');

  async function onSubmit(event) {
    event.preventDefault();
    setError('');
    try {
      await register(form);
      navigate('/perfil', { replace: true });
    } catch (err) {
      setError(getErrorMessage(err, 'No pudimos registrar la cuenta.'));
    }
  }

  return (
    <section className="mx-auto max-w-5xl overflow-hidden rounded-[2rem] border border-white/70 bg-white/85 shadow-soft backdrop-blur-sm">
      <div className="grid md:grid-cols-[1fr_1.1fr]">
        <div className="p-8 md:p-10">
          <div className="flex items-center gap-3">
            <span className="grid h-12 w-12 place-items-center rounded-2xl bg-white shadow-soft ring-1 ring-lavender-100">
              <img src="/unicaribe-logo.png" alt="Unicaribe" className="h-9 w-9 object-contain" />
            </span>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-lavender-700">Unicaribe</p>
              <p className="font-display text-base font-bold text-ink-900">Biblioteca Virtual</p>
            </div>
          </div>

          <h1 className="mt-6 font-display text-3xl font-extrabold text-ink-900">Crear cuenta lectora</h1>
          <p className="mt-2 text-sm text-ink-500">
            Tu acceso para reservar libros, gestionar préstamos y recibir avisos.
          </p>

          {error && <div className="mt-4"><StatusMessage type="error">{error}</StatusMessage></div>}

          <form className="mt-6 grid gap-4" onSubmit={onSubmit}>
            <input
              required
              className="input-pastel"
              placeholder="Nombre completo"
              value={form.nombre}
              onChange={(e) => setForm({ ...form, nombre: e.target.value })}
            />
            <input
              required
              type="email"
              className="input-pastel"
              placeholder="Email institucional"
              value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
            />
            <input
              required
              minLength={8}
              type="password"
              className="input-pastel"
              placeholder="Contraseña (mínimo 8 caracteres)"
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
            />
            <div className="grid gap-4 sm:grid-cols-2">
              <input
                className="input-pastel"
                placeholder="Teléfono"
                value={form.telefono}
                onChange={(e) => setForm({ ...form, telefono: e.target.value })}
              />
              <input
                className="input-pastel"
                placeholder="Dirección"
                value={form.direccion}
                onChange={(e) => setForm({ ...form, direccion: e.target.value })}
              />
            </div>
            <button
              disabled={loading}
              className="rounded-full bg-gradient-to-r from-lavender-500 to-peach-500 px-4 py-2.5 font-semibold text-white shadow-glow transition hover:opacity-95 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {loading ? 'Registrando...' : 'Registrarme'}
            </button>
          </form>

          <p className="mt-6 text-sm text-ink-500">
            ¿Ya tienes cuenta?{' '}
            <Link className="font-semibold text-lavender-700 hover:text-lavender-500" to="/login">
              Ingresa
            </Link>
          </p>
        </div>

        <aside className="relative hidden flex-col justify-between bg-pastel-hero p-10 md:flex">
          <div className="grid h-16 w-16 place-items-center rounded-3xl bg-white shadow-soft">
            <img src="/unicaribe-logo.png" alt="Unicaribe" className="h-11 w-11 object-contain" />
          </div>
          <div>
            <h2 className="font-display text-3xl font-extrabold leading-tight text-ink-900">
              Súmate a la<br />Biblioteca Virtual.
            </h2>
            <p className="mt-3 text-sm text-ink-700/80">
              Más de mil títulos, accesos digitales y herramientas para acompañar tu trayectoria
              académica en la Universidad del Caribe.
            </p>
          </div>
          <ul className="space-y-2 text-sm text-ink-700">
            <li className="flex items-center gap-2"><span className="h-1.5 w-1.5 rounded-full bg-lavender-500" /> Hasta 3 préstamos activos</li>
            <li className="flex items-center gap-2"><span className="h-1.5 w-1.5 rounded-full bg-peach-500" /> Reservas con prioridad de retiro</li>
            <li className="flex items-center gap-2"><span className="h-1.5 w-1.5 rounded-full bg-mint-500" /> Historial y multas siempre a mano</li>
          </ul>
        </aside>
      </div>
    </section>
  );
}
