import { useEffect, useState } from 'react';
import AdminTabs from '../components/AdminTabs.jsx';
import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { getErrorMessage } from '../services/api.js';
import { fetchAudit } from '../services/adminService.js';
import { formatDate } from '../utils/formatters.js';

export default function AdminAuditPage() {
  const [state, setState] = useState({ events: [], loading: true, error: '' });

  useEffect(() => {
    fetchAudit()
      .then((events) => setState({ events, loading: false, error: '' }))
      .catch((err) =>
        setState({ events: [], loading: false, error: getErrorMessage(err, 'No se pudo cargar auditoría.') })
      );
  }, []);

  return (
    <section className="space-y-6">
      <AdminTabs />
      <PageHeader eyebrow="Administración" title="Auditoría operativa">
        Historial append-only de acciones relevantes. Esta vista es para demo y revisión interna.
      </PageHeader>

      {state.error && <StatusMessage type="error">{state.error}</StatusMessage>}
      {state.loading ? (
        <StatusMessage>Cargando auditoría...</StatusMessage>
      ) : state.events.length === 0 ? (
        <StatusMessage>No hay eventos registrados.</StatusMessage>
      ) : (
        <div className="overflow-hidden rounded-3xl border border-white/70 bg-white/85 shadow-soft backdrop-blur-sm">
          <table className="min-w-full text-left text-sm">
            <thead className="bg-lavender-50/70 text-ink-700">
              <tr>
                <th className="p-4 font-semibold">Fecha</th>
                <th className="p-4 font-semibold">Actor</th>
                <th className="p-4 font-semibold">Acción</th>
                <th className="p-4 font-semibold">Entidad</th>
                <th className="p-4 font-semibold">Resultado</th>
              </tr>
            </thead>
            <tbody>
              {state.events.map((event) => (
                <tr key={event.id} className="border-t border-lavender-100/60">
                  <td className="p-4 text-ink-500">{formatDate(event.fecha)}</td>
                  <td className="p-4 font-semibold text-ink-900">{event.actorNombre ?? 'Sistema'}</td>
                  <td className="p-4">
                    <span className="inline-flex items-center rounded-full bg-lavender-100 px-2.5 py-0.5 text-[11px] font-mono text-lavender-700">
                      {event.accion}
                    </span>
                  </td>
                  <td className="p-4 text-ink-700">{`${event.entidad} #${event.entidadId}`}</td>
                  <td className="p-4 text-ink-700">{event.resultado}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
