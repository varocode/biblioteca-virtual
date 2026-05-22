import { useEffect, useState } from 'react';
import AdminTabs from '../components/AdminTabs.jsx';
import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { getErrorMessage } from '../services/api.js';
import { approveLoan, fetchLoans, fetchReservations, prepareReservationPickup, returnLoan } from '../services/circulationService.js';
import { enumLabel, formatDate, loanStatus, reservationStatus } from '../utils/formatters.js';

const approveBtn = 'rounded-full bg-gradient-to-r from-lavender-500 to-lavender-600 px-3 py-2 text-xs font-semibold text-white shadow-glow transition disabled:opacity-50';
const returnBtn = 'rounded-full bg-gradient-to-r from-mint-500 to-mint-700 px-3 py-2 text-xs font-semibold text-white shadow-glow transition disabled:opacity-50';
const pickupBtn = 'rounded-full border border-lavender-200 bg-white px-3 py-2 text-xs font-semibold text-ink-900 transition hover:border-lavender-400 hover:bg-lavender-50 disabled:opacity-50';

export default function AdminCirculationPage() {
  const [state, setState] = useState({ loans: [], reservations: [], loading: true, saving: false, error: '', success: '' });

  const load = () =>
    Promise.all([fetchLoans(), fetchReservations()])
      .then(([loans, reservations]) => setState((s) => ({ ...s, loans, reservations, loading: false })))
      .catch((err) => setState((s) => ({ ...s, loading: false, error: getErrorMessage(err, 'No se pudo cargar circulación.') })));
  useEffect(() => {
    load();
  }, []);

  async function run(work, success) {
    setState((s) => ({ ...s, saving: true, error: '', success: '' }));
    try {
      await work();
      await load();
      setState((s) => ({ ...s, saving: false, success }));
    } catch (err) {
      setState((s) => ({ ...s, saving: false, error: getErrorMessage(err) }));
    }
  }

  const pending = state.loans.filter((loan) => enumLabel(loan.estado, loanStatus) === 'Pendiente');
  const active = state.loans.filter((loan) => ['Activo', 'Vencido'].includes(enumLabel(loan.estado, loanStatus)));
  const reservations = state.reservations.filter((reservation) => ['Activa', 'Asignada'].includes(enumLabel(reservation.estado, reservationStatus)));

  return (
    <section className="space-y-6">
      <AdminTabs />
      <PageHeader eyebrow="Administración" title="Circulación">
        Aprueba solicitudes, asigna ejemplares, procesa devoluciones y prepara reservas para retiro.
      </PageHeader>

      {state.error && <StatusMessage type="error">{state.error}</StatusMessage>}
      {state.success && <StatusMessage type="success">{state.success}</StatusMessage>}

      {state.loading ? (
        <StatusMessage>Cargando circulación...</StatusMessage>
      ) : (
        <div className="grid gap-6 lg:grid-cols-3">
          <Panel title="Solicitudes pendientes" empty="Sin solicitudes." accent="lavender">
            {pending.map((loan) => (
              <Loan key={loan.id} loan={loan}>
                <button
                  disabled={state.saving}
                  onClick={() => run(() => approveLoan(loan.id), 'Préstamo aprobado.')}
                  className={approveBtn}
                >
                  Aprobar y asignar
                </button>
              </Loan>
            ))}
          </Panel>
          <Panel title="Préstamos activos" empty="Sin préstamos activos." accent="mint">
            {active.map((loan) => (
              <Loan key={loan.id} loan={loan}>
                <button
                  disabled={state.saving}
                  onClick={() => run(() => returnLoan(loan.id), 'Devolución procesada.')}
                  className={returnBtn}
                >
                  Procesar devolución
                </button>
              </Loan>
            ))}
          </Panel>
          <Panel title="Reservas" empty="Sin reservas activas." accent="sky">
            {reservations.map((reservation) => (
              <article
                key={reservation.id}
                className="rounded-2xl border border-white/70 bg-white/85 p-4 shadow-soft backdrop-blur-sm"
              >
                <h3 className="font-display font-bold text-ink-900">{reservation.libro?.titulo}</h3>
                <p className="mt-1 text-sm text-ink-500">
                  {reservation.usuario?.nombre} · {enumLabel(reservation.estado, reservationStatus)} · retirar hasta{' '}
                  {formatDate(reservation.fechaExpiracion)}
                </p>
                {enumLabel(reservation.estado, reservationStatus) === 'Activa' && (
                  <button
                    disabled={state.saving}
                    onClick={() => run(() => prepareReservationPickup(reservation.id), 'Reserva preparada para retiro.')}
                    className={`mt-3 ${pickupBtn}`}
                  >
                    Preparar retiro
                  </button>
                )}
              </article>
            ))}
          </Panel>
        </div>
      )}
    </section>
  );
}

const ACCENT_DOTS = {
  lavender: 'bg-lavender-500',
  mint: 'bg-mint-500',
  sky: 'bg-sky-500',
  peach: 'bg-peach-500'
};

function Panel({ title, empty, children, accent = 'lavender' }) {
  const items = Array.isArray(children) ? children.filter(Boolean) : children;
  return (
    <div className="space-y-3">
      <h2 className="inline-flex items-center gap-2 font-display text-lg font-bold text-ink-900">
        <span className={`h-2 w-2 rounded-full ${ACCENT_DOTS[accent]}`} />
        {title}
      </h2>
      {items?.length ? items : <StatusMessage>{empty}</StatusMessage>}
    </div>
  );
}

function Loan({ loan, children }) {
  return (
    <article className="rounded-2xl border border-white/70 bg-white/85 p-4 shadow-soft backdrop-blur-sm">
      <h3 className="font-display font-bold text-ink-900">{loan.libro?.titulo}</h3>
      <p className="mt-1 text-sm text-ink-500">
        {loan.usuario?.nombre} · {enumLabel(loan.estado, loanStatus)} · ejemplar {loan.ejemplar?.codigo ?? 'pendiente'} ·
        vence {formatDate(loan.fechaDevolucionEsperada)}
      </p>
      <div className="mt-3">{children}</div>
    </article>
  );
}
