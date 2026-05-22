import { useEffect, useState } from 'react';
import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { getErrorMessage } from '../services/api.js';
import { createLoan, fetchLoans, renewLoan } from '../services/circulationService.js';
import { enumLabel, formatDate, loanStatus } from '../utils/formatters.js';

const LOAN_TONES = {
  Activo: 'bg-mint-100 text-mint-700',
  Vencido: 'bg-peach-100 text-peach-700',
  Devuelto: 'bg-lavender-100 text-lavender-700',
  Renovado: 'bg-sky-100 text-sky-700'
};

export default function ReaderLoansPage() {
  const [loans, setLoans] = useState([]);
  const [bookId, setBookId] = useState('');
  const [status, setStatus] = useState({ loading: true, saving: false, error: '', success: '' });

  const load = () =>
    fetchLoans()
      .then(setLoans)
      .catch((err) =>
        setStatus((s) => ({ ...s, error: getErrorMessage(err, 'No se pudieron cargar los préstamos.') }))
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
      await createLoan(bookId);
      setBookId('');
      await load();
      setStatus({ loading: false, saving: false, error: '', success: 'Préstamo solicitado correctamente.' });
    } catch (err) {
      setStatus({
        loading: false,
        saving: false,
        error: getErrorMessage(err, 'No se pudo solicitar el préstamo.'),
        success: ''
      });
    }
  }

  async function action(fn, id, success) {
    setStatus((s) => ({ ...s, saving: true, error: '', success: '' }));
    try {
      await fn(id);
      await load();
      setStatus({ loading: false, saving: false, error: '', success });
    } catch (err) {
      setStatus({ loading: false, saving: false, error: getErrorMessage(err), success: '' });
    }
  }

  return (
    <section className="space-y-6">
      <PageHeader eyebrow="Préstamos" title="Mis préstamos">
        Pide libros desde el catálogo. La biblioteca aprueba el retiro, asigna el ejemplar y procesa la devolución.
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
            className="rounded-full bg-gradient-to-r from-lavender-500 to-lavender-600 px-5 py-2 text-sm font-semibold text-white shadow-glow transition hover:from-lavender-600 hover:to-lavender-700 disabled:cursor-not-allowed disabled:opacity-50"
            disabled={status.saving}
          >
            Enviar solicitud
          </button>
        </div>
      </form>

      {status.error && <StatusMessage type="error">{status.error}</StatusMessage>}
      {status.success && <StatusMessage type="success">{status.success}</StatusMessage>}
      {status.loading ? (
        <StatusMessage>Cargando préstamos...</StatusMessage>
      ) : loans.length === 0 ? (
        <StatusMessage>No tienes préstamos todavía.</StatusMessage>
      ) : (
        <div className="space-y-3">
          {loans.map((loan) => (
            <LoanRow
              key={loan.id}
              loan={loan}
              disabled={status.saving}
              onRenew={() => action(renewLoan, loan.id, 'Préstamo renovado.')}
            />
          ))}
        </div>
      )}
    </section>
  );
}

function LoanRow({ loan, disabled, onRenew }) {
  const label = enumLabel(loan.estado, loanStatus);
  const isActive = label === 'Activo';
  const tone = LOAN_TONES[label] || 'bg-lavender-100 text-lavender-700';
  return (
    <article className="rounded-2xl border border-white/70 bg-white/85 p-5 shadow-soft backdrop-blur-sm">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="min-w-0">
          <h2 className="font-display text-lg font-bold text-ink-900">
            {loan.libro?.titulo ?? `Libro #${loan.libro?.id}`}
          </h2>
          <p className="mt-1 flex flex-wrap items-center gap-2 text-sm text-ink-500">
            <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-[11px] font-semibold ${tone}`}>
              {label}
            </span>
            <span>Ejemplar {loan.ejemplar?.codigo ?? 'pendiente'}</span>
            <span aria-hidden>·</span>
            <span>vence {formatDate(loan.fechaDevolucionEsperada)}</span>
          </p>
        </div>
        {isActive && (
          <button
            className="rounded-full border border-lavender-200 bg-white px-4 py-2 text-sm font-semibold text-ink-900 transition hover:border-lavender-400 hover:bg-lavender-50 disabled:cursor-not-allowed disabled:opacity-50"
            disabled={disabled}
            onClick={onRenew}
          >
            Renovar
          </button>
        )}
      </div>
    </article>
  );
}
