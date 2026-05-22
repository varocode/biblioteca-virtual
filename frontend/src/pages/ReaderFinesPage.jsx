import { useEffect, useState } from 'react';
import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { getErrorMessage } from '../services/api.js';
import { fetchFines, payFine } from '../services/circulationService.js';
import { enumLabel, fineStatus, formatDate, formatMoney } from '../utils/formatters.js';

const FINE_TONES = {
  Pendiente: 'bg-peach-100 text-peach-700',
  Pagada: 'bg-mint-100 text-mint-700',
  Anulada: 'bg-lavender-100 text-lavender-700'
};

export default function ReaderFinesPage() {
  const [state, setState] = useState({ fines: [], loading: true, saving: false, error: '', success: '' });

  const load = () =>
    fetchFines()
      .then((fines) => setState((s) => ({ ...s, fines, loading: false })))
      .catch((err) =>
        setState((s) => ({
          ...s,
          fines: [],
          loading: false,
          error: getErrorMessage(err, 'No se pudieron cargar las multas.')
        }))
      );
  useEffect(() => {
    load();
  }, []);

  async function simulatePayment(id, aprobar) {
    setState((s) => ({ ...s, saving: true, error: '', success: '' }));
    try {
      const intento = await payFine(id, aprobar);
      await load();
      setState((s) => ({
        ...s,
        saving: false,
        success: aprobar
          ? `Pago simulado aprobado. Referencia ${intento.referencia}.`
          : `Pago simulado rechazado. Referencia ${intento.referencia}.`
      }));
    } catch (err) {
      setState((s) => ({ ...s, saving: false, error: getErrorMessage(err) }));
    }
  }

  const pending = state.fines
    .filter((fine) => enumLabel(fine.estado, fineStatus) === 'Pendiente')
    .reduce((sum, fine) => sum + Number(fine.monto ?? 0), 0);

  return (
    <section className="space-y-6">
      <PageHeader eyebrow="Multas" title="Mis multas">
        Paga multas con una pasarela simulada: no hay bancos reales, solo referencias y recibos de demo.
      </PageHeader>

      {state.error && <StatusMessage type="error">{state.error}</StatusMessage>}
      {state.success && <StatusMessage type="success">{state.success}</StatusMessage>}

      <div className={`rounded-3xl border border-white/70 p-6 shadow-soft backdrop-blur-sm ${pending > 0 ? 'bg-gradient-to-br from-peach-100 via-white to-white' : 'bg-gradient-to-br from-mint-100 via-white to-white'}`}>
        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-ink-500">Total pendiente</p>
        <p className="mt-2 font-display text-4xl font-extrabold text-ink-900">{formatMoney(pending)}</p>
      </div>

      {state.loading ? (
        <StatusMessage>Cargando multas...</StatusMessage>
      ) : state.fines.length === 0 ? (
        <StatusMessage>No tienes multas registradas.</StatusMessage>
      ) : (
        <div className="overflow-hidden rounded-3xl border border-white/70 bg-white/85 shadow-soft backdrop-blur-sm">
          <table className="min-w-full text-left text-sm">
            <thead className="bg-lavender-50/70 text-ink-700">
              <tr>
                <th className="p-4 font-semibold">Préstamo</th>
                <th className="p-4 font-semibold">Días</th>
                <th className="p-4 font-semibold">Monto</th>
                <th className="p-4 font-semibold">Estado</th>
                <th className="p-4 font-semibold">Recibo / intentos</th>
                <th className="p-4 font-semibold">Acción</th>
              </tr>
            </thead>
            <tbody>
              {state.fines.map((fine) => {
                const status = enumLabel(fine.estado, fineStatus);
                const tone = FINE_TONES[status] || 'bg-lavender-100 text-lavender-700';
                return (
                  <tr key={fine.id} className="border-t border-lavender-100/60">
                    <td className="p-4 font-semibold text-ink-900">#{fine.prestamoId}</td>
                    <td className="p-4 text-ink-700">{fine.diasRetraso}</td>
                    <td className="p-4 font-semibold text-ink-900">{formatMoney(fine.monto)}</td>
                    <td className="p-4">
                      <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-[11px] font-semibold ${tone}`}>
                        {status}
                      </span>
                    </td>
                    <td className="p-4 text-sm text-ink-700">
                      <p>{fine.pago?.recibo ?? '—'}</p>
                      <p className="text-xs text-ink-500">{fine.intentosPago?.[0]?.referencia ?? 'Sin intentos'}</p>
                    </td>
                    <td className="p-4">
                      {status === 'Pendiente' ? (
                        <div className="flex flex-wrap gap-2">
                          <button
                            disabled={state.saving}
                            onClick={() => simulatePayment(fine.id, true)}
                            className="rounded-full bg-gradient-to-r from-mint-500 to-mint-700 px-3 py-2 text-xs font-semibold text-white shadow-glow disabled:opacity-50"
                          >
                            Aprobar pago demo
                          </button>
                          <button
                            disabled={state.saving}
                            onClick={() => simulatePayment(fine.id, false)}
                            className="rounded-full border border-lavender-200 bg-white px-3 py-2 text-xs font-semibold text-ink-700 hover:border-peach-300 hover:bg-peach-100 hover:text-peach-700"
                          >
                            Simular rechazo
                          </button>
                        </div>
                      ) : (
                        <span className="text-sm text-ink-500">Pagada {formatDate(fine.fechaPago)}</span>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
