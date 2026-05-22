import { Link } from 'react-router-dom';
import { useState } from 'react';
import StatusMessage from './StatusMessage.jsx';
import { useAuth } from '../context/AuthContext.jsx';
import { getErrorMessage } from '../services/api.js';
import { createLoan, createReservation } from '../services/circulationService.js';

export default function BookActions({ book, onDone, compact = false }) {
  const { isAuthenticated } = useAuth();
  const [status, setStatus] = useState({ saving: false, error: '', success: '' });
  const available = Number(book.ejemplaresDisponibles ?? book.disponibles ?? 0) > 0;
  const actionLabel = available ? 'Solicitar préstamo' : 'Reservar';

  if (!isAuthenticated) {
    return (
      <Link
        className="inline-flex items-center gap-1 text-sm font-semibold text-lavender-700 hover:text-lavender-500"
        to="/login"
      >
        Ingresa para pedirlo →
      </Link>
    );
  }

  async function submit() {
    setStatus({ saving: true, error: '', success: '' });
    try {
      if (available) {
        await createLoan(book.id);
      } else {
        await createReservation(book.id);
      }
      await onDone?.();
      setStatus({ saving: false, error: '', success: available ? 'Préstamo solicitado.' : 'Reserva creada.' });
    } catch (err) {
      setStatus({
        saving: false,
        error: getErrorMessage(err, available ? 'No se pudo solicitar el préstamo.' : 'No se pudo crear la reserva.'),
        success: ''
      });
    }
  }

  const baseBtn = 'inline-flex items-center justify-center rounded-full px-4 py-2 text-sm font-semibold text-white shadow-glow transition disabled:cursor-not-allowed disabled:opacity-50';
  const tone = available
    ? 'bg-gradient-to-r from-lavender-500 to-lavender-600 hover:from-lavender-600 hover:to-lavender-700'
    : 'bg-gradient-to-r from-peach-300 to-peach-500 hover:from-peach-500 hover:to-peach-700';

  return (
    <div className={compact ? 'space-y-2' : 'space-y-3'}>
      <button
        className={`${baseBtn} ${tone}`}
        disabled={status.saving}
        onClick={submit}
        type="button"
      >
        {status.saving ? 'Procesando...' : actionLabel}
      </button>
      {status.error && <StatusMessage type="error">{status.error}</StatusMessage>}
      {status.success && <StatusMessage type="success">{status.success}</StatusMessage>}
    </div>
  );
}
