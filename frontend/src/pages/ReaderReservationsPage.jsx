import { useEffect, useState } from 'react';
import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { getErrorMessage } from '../services/api.js';
import { cancelReservation, createReservation, fetchReservations } from '../services/circulationService.js';
import { enumLabel, formatDate, reservationStatus } from '../utils/formatters.js';

const RESERVATION_TONES = {
  Activa: 'bg-sky-100 text-sky-700',
  Asignada: 'bg-mint-100 text-mint-700',
  Cancelada: 'bg-peach-100 text-peach-700',
  Expirada: 'bg-peach-100 text-peach-700',
  Cumplida: 'bg-lavender-100 text-lavender-700'
};

export default function ReaderReservationsPage() {
  const [reservations, setReservations] = useState([]);
  const [bookId, setBookId] = useState('');
  const [status, setStatus] = useState({ loading: true, saving: false, error: '', success: '' });

  const load = () =>
    fetchReservations()
      .then(setReservations)
      .catch((err) =>
        setStatus((s) => ({ ...s, error: getErrorMessage(err, 'No se pudieron cargar las reservas.') }))
      )
      .finally(() => setStatus((s) => ({ ...s, loading: false })));
  useEffect(() => {
    load();
  }, []);

  async function submit(event) {
    event.preventDefault();
    if (!bookId) return setStatus((s) => ({ ...s, error: 'Ingresa el ID del libro.' }));
    setStatus({ loading: false, saving: true, error: '', success: '' });
    try {
      await createReservation(bookId);
      setBookId('');
      await load();
      setStatus({ loading: false, saving: false, error: '', success: 'Reserva creada correctamente.' });
    } catch (err) {
      setStatus({
        loading: false,
        saving: false,
        error: getErrorMessage(err, 'No se pudo crear la reserva.'),
        success: ''
      });
    }
  }

  async function cancel(id) {
    if (!confirm('¿Cancelar esta reserva?')) return;
    try {
      await cancelReservation(id);
      await load();
      setStatus({ loading: false, saving: false, error: '', success: 'Reserva cancelada.' });
    } catch (err) {
      setStatus({ loading: false, saving: false, error: getErrorMessage(err), success: '' });
    }
  }

  return (
    <section className="space-y-6">
      <PageHeader eyebrow="Reservas" title="Mis reservas">
        Reserva libros desde el catálogo. Cuando la biblioteca prepare el ejemplar, tienes 48 h para retirarlo.
      </PageHeader>

      <form
        onSubmit={submit}
        className="rounded-3xl border border-white/70 bg-white/85 p-5 shadow-soft backdrop-blur-sm"
      >
        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-ink-500">Atajo opcional por ID</p>
        <div className="mt-3 flex flex-wrap gap-3">
          <input
            className="input-pastel min-w-48 flex-1"
            type="number"
            min="1"
            placeholder="ID del libro"
            value={bookId}
            onChange={(e) => setBookId(e.target.value)}
          />
          <button
            className="rounded-full bg-gradient-to-r from-lavender-500 to-peach-500 px-5 py-2 text-sm font-semibold text-white shadow-glow transition hover:opacity-95 disabled:cursor-not-allowed disabled:opacity-50"
            disabled={status.saving}
          >
            Reservar
          </button>
        </div>
      </form>

      {status.error && <StatusMessage type="error">{status.error}</StatusMessage>}
      {status.success && <StatusMessage type="success">{status.success}</StatusMessage>}

      {status.loading ? (
        <StatusMessage>Cargando reservas...</StatusMessage>
      ) : reservations.length === 0 ? (
        <StatusMessage>No tienes reservas activas.</StatusMessage>
      ) : (
        <div className="space-y-3">
          {reservations.map((reservation) => {
            const label = enumLabel(reservation.estado, reservationStatus);
            const tone = RESERVATION_TONES[label] || 'bg-lavender-100 text-lavender-700';
            const canCancel = ['Activa', 'Asignada'].includes(label);
            return (
              <article
                key={reservation.id}
                className="rounded-2xl border border-white/70 bg-white/85 p-5 shadow-soft backdrop-blur-sm"
              >
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div className="min-w-0">
                    <h2 className="font-display text-lg font-bold text-ink-900">
                      {reservation.libro?.titulo ?? `Libro #${reservation.libro?.id}`}
                    </h2>
                    <p className="mt-1 flex flex-wrap items-center gap-2 text-sm text-ink-500">
                      <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-[11px] font-semibold ${tone}`}>
                        {label}
                      </span>
                      <span>Ejemplar {reservation.ejemplar?.codigo ?? 'pendiente'}</span>
                      <span aria-hidden>·</span>
                      <span>posición {reservation.posicionCola}</span>
                      <span aria-hidden>·</span>
                      <span>retirar hasta {formatDate(reservation.fechaExpiracion)}</span>
                    </p>
                  </div>
                  {canCancel && (
                    <button
                      className="rounded-full border border-lavender-200 bg-white px-4 py-2 text-sm font-semibold text-ink-900 transition hover:border-peach-300 hover:bg-peach-100 hover:text-peach-700"
                      onClick={() => cancel(reservation.id)}
                    >
                      Cancelar
                    </button>
                  )}
                </div>
              </article>
            );
          })}
        </div>
      )}
    </section>
  );
}
