import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { useAuth } from '../context/AuthContext.jsx';
import { normalizeRole } from '../utils/roles.js';

export default function ProfilePage() {
  const { user, refreshProfile, loading } = useAuth();
  const initials = (user?.nombre || 'U')
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase())
    .join('');

  return (
    <section className="space-y-6">
      <PageHeader
        eyebrow="Mi perfil"
        title="Datos personales"
        action={
          <button
            className="rounded-full border border-lavender-200 bg-white px-4 py-2 text-sm font-semibold text-ink-900 transition hover:border-lavender-400 hover:bg-lavender-50 disabled:opacity-50"
            disabled={loading}
            onClick={refreshProfile}
          >
            Actualizar
          </button>
        }
      >
        Datos básicos de la sesión autenticada.
      </PageHeader>

      {!user ? (
        <StatusMessage>No hay datos de usuario.</StatusMessage>
      ) : (
        <article className="overflow-hidden rounded-3xl border border-white/70 bg-white/85 shadow-soft backdrop-blur-sm">
          <header className="flex items-center gap-4 bg-pastel-hero p-6">
            <span className="grid h-16 w-16 place-items-center rounded-2xl bg-white text-2xl font-bold text-lavender-700 shadow-soft">
              {initials || 'U'}
            </span>
            <div>
              <p className="font-display text-2xl font-extrabold text-ink-900">{user.nombre}</p>
              <p className="text-sm text-ink-700">{user.email}</p>
              <span className="mt-2 inline-flex items-center gap-2 rounded-full bg-white/80 px-3 py-0.5 text-[11px] font-semibold uppercase tracking-wider text-lavender-700 backdrop-blur">
                <span className="h-1.5 w-1.5 rounded-full bg-lavender-500" />
                {normalizeRole(user.rol)}
              </span>
            </div>
          </header>
          <dl className="grid gap-3 p-6 md:grid-cols-2">
            <ProfileField label="Estado" value={user.activo ? 'Activo' : 'Inactivo'} tone={user.activo ? 'mint' : 'peach'} />
            <ProfileField label="Teléfono" value={user.telefono || '—'} />
            <ProfileField label="Dirección" value={user.direccion || '—'} />
            <ProfileField label="Email" value={user.email} />
          </dl>
        </article>
      )}
    </section>
  );
}

const TONES = {
  default: 'bg-cream-50',
  mint: 'bg-mint-100/60',
  peach: 'bg-peach-100/60'
};

function ProfileField({ label, value, tone = 'default' }) {
  return (
    <div className={`rounded-2xl p-4 ${TONES[tone]}`}>
      <dt className="text-[11px] font-semibold uppercase tracking-wider text-ink-500">{label}</dt>
      <dd className="mt-1 font-semibold text-ink-900">{value}</dd>
    </div>
  );
}
