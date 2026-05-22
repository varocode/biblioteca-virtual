import { useEffect, useState } from 'react';
import AdminTabs from '../components/AdminTabs.jsx';
import DataCard from '../components/DataCard.jsx';
import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { getErrorMessage } from '../services/api.js';
import { fetchDashboard } from '../services/adminService.js';
import { formatMoney } from '../utils/formatters.js';

const CHART_TONES = ['from-lavender-300 to-lavender-500', 'from-peach-300 to-peach-500', 'from-mint-300 to-mint-500', 'from-sky-300 to-sky-500'];

export default function AdminDashboardPage() {
  const [state, setState] = useState({ data: null, loading: true, error: '' });

  useEffect(() => {
    fetchDashboard()
      .then((data) => setState({ data, loading: false, error: '' }))
      .catch((err) =>
        setState({ data: null, loading: false, error: getErrorMessage(err, 'No se pudo cargar el dashboard.') })
      );
  }, []);

  const d = state.data;

  return (
    <section className="space-y-6">
      <AdminTabs />
      <PageHeader eyebrow="Administración" title="Dashboard">
        Métricas operativas y reportes de la biblioteca.
      </PageHeader>
      {state.error && <StatusMessage type="error">{state.error}</StatusMessage>}
      {state.loading ? (
        <StatusMessage>Cargando dashboard...</StatusMessage>
      ) : (
        d && (
          <>
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              <DataCard title="Libros" value={d.totalLibros} tone="lavender" icon="📚" />
              <DataCard title="Usuarios" value={d.totalUsuarios} tone="sky" icon="👥" />
              <DataCard title="Préstamos activos" value={d.prestamosActivos} tone="mint" icon="📖" />
              <DataCard title="Préstamos vencidos" value={d.prestamosVencidos} tone="peach" icon="⏰" />
              <DataCard title="Reservas activas" value={d.reservasActivas} tone="sky" icon="🔖" />
              <DataCard
                title="Multas pendientes"
                value={formatMoney(d.montoMultasPendientes)}
                tone="peach"
                icon="💳"
              >
                {d.multasPendientes} pendiente(s)
              </DataCard>
            </div>
            <div className="grid gap-4 lg:grid-cols-2">
              <Chart title="Top libros" items={d.topLibros} toneIndex={0} />
              <Chart title="Usuarios activos" items={d.usuariosActivos} toneIndex={1} />
              <Chart title="Préstamos por mes" items={d.prestamosPorMes} toneIndex={2} />
              <Chart title="Categorías populares" items={d.categoriasPopulares} toneIndex={3} />
            </div>
          </>
        )
      )}
    </section>
  );
}

function Chart({ title, items = [], toneIndex = 0 }) {
  const max = Math.max(1, ...items.map((item) => item.valor ?? 0));
  const tone = CHART_TONES[toneIndex % CHART_TONES.length];
  return (
    <article className="rounded-3xl border border-white/70 bg-white/85 p-5 shadow-soft backdrop-blur-sm">
      <h2 className="font-display text-lg font-bold text-ink-900">{title}</h2>
      {items.length === 0 ? (
        <p className="mt-3 text-sm text-ink-500">Sin datos.</p>
      ) : (
        <div className="mt-4 space-y-3">
          {items.map((item) => (
            <div key={item.etiqueta}>
              <div className="mb-1 flex justify-between text-sm">
                <span className="text-ink-700">{item.etiqueta}</span>
                <strong className="text-ink-900">{item.valor}</strong>
              </div>
              <div className="h-2.5 overflow-hidden rounded-full bg-lavender-100/60">
                <div
                  className={`h-full rounded-full bg-gradient-to-r ${tone}`}
                  style={{ width: `${Math.max(8, ((item.valor ?? 0) / max) * 100)}%` }}
                />
              </div>
            </div>
          ))}
        </div>
      )}
    </article>
  );
}
