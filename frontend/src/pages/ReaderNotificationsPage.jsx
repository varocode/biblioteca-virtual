import { useEffect, useState } from 'react';
import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { getErrorMessage } from '../services/api.js';
import { fetchNotifications } from '../services/circulationService.js';
import { formatDate } from '../utils/formatters.js';

const NOTIFICATION_TONES = {
  PRESTAMO: 'bg-lavender-100 text-lavender-700',
  RESERVA: 'bg-sky-100 text-sky-700',
  MULTA: 'bg-peach-100 text-peach-700',
  PAGO: 'bg-mint-100 text-mint-700'
};

export default function ReaderNotificationsPage() {
  const [state, setState] = useState({ notifications: [], loading: true, error: '' });

  useEffect(() => {
    fetchNotifications()
      .then((notifications) => setState({ notifications, loading: false, error: '' }))
      .catch((err) =>
        setState({
          notifications: [],
          loading: false,
          error: getErrorMessage(err, 'No se pudieron cargar las notificaciones.')
        })
      );
  }, []);

  return (
    <section className="space-y-6">
      <PageHeader eyebrow="Notificaciones" title="Mensajes de biblioteca">
        Bandeja de solo lectura para eventos de préstamos, reservas y pagos. Es una simulación: no se envía email ni SMS.
      </PageHeader>

      {state.error && <StatusMessage type="error">{state.error}</StatusMessage>}
      {state.loading ? (
        <StatusMessage>Cargando notificaciones...</StatusMessage>
      ) : state.notifications.length === 0 ? (
        <StatusMessage>No tienes notificaciones.</StatusMessage>
      ) : (
        <div className="space-y-3">
          {state.notifications.map((notification) => {
            const tone = NOTIFICATION_TONES[notification.tipo?.toUpperCase()] || 'bg-lavender-100 text-lavender-700';
            return (
              <article
                key={notification.id}
                className="rounded-2xl border border-white/70 bg-white/85 p-5 shadow-soft backdrop-blur-sm"
              >
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div className="min-w-0">
                    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-[11px] font-bold uppercase tracking-wider ${tone}`}>
                      {notification.tipo}
                    </span>
                    <h2 className="mt-2 font-display text-lg font-bold text-ink-900">{notification.titulo}</h2>
                  </div>
                  <span className="text-xs font-semibold text-ink-500">{formatDate(notification.fecha)}</span>
                </div>
                <p className="mt-3 text-sm leading-relaxed text-ink-700">{notification.mensaje}</p>
                {notification.referencia && (
                  <p className="mt-3 text-xs text-ink-500">Referencia: {notification.referencia}</p>
                )}
              </article>
            );
          })}
        </div>
      )}
    </section>
  );
}
