import { useEffect, useState } from 'react';
import DataCard from '../components/DataCard.jsx';
import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { getErrorMessage } from '../services/api.js';
import { fetchFines, fetchLoans, fetchReservations } from '../services/circulationService.js';
import { enumLabel, fineStatus, formatMoney, loanStatus, reservationStatus } from '../utils/formatters.js';

export default function ReaderDashboardPage() {
  const [state, setState] = useState({ loans: [], reservations: [], fines: [], loading: true, error: '' });

  useEffect(() => {
    Promise.all([fetchLoans(), fetchReservations(), fetchFines()])
      .then(([loans, reservations, fines]) => setState({ loans, reservations, fines, loading: false, error: '' }))
      .catch((err) => setState((current) => ({ ...current, loading: false, error: getErrorMessage(err, 'No se pudo cargar tu biblioteca.') })));
  }, []);

  const activeLoans = state.loans.filter((loan) => enumLabel(loan.estado, loanStatus) === 'Activo').length;
  const activeReservations = state.reservations.filter((reservation) =>
    ['Activa', 'Asignada'].includes(enumLabel(reservation.estado, reservationStatus))
  ).length;
  const pendingFines = state.fines.filter((fine) => enumLabel(fine.estado, fineStatus) === 'Pendiente');
  const pendingAmount = pendingFines.reduce((sum, fine) => sum + Number(fine.monto ?? 0), 0);

  return (
    <section className="space-y-8">
      <PageHeader eyebrow="Área lectora" title="Mi biblioteca">
        Resumen de préstamos, reservas y multas pendientes.
      </PageHeader>
      {state.error && <StatusMessage type="error">{state.error}</StatusMessage>}
      {state.loading ? (
        <StatusMessage>Cargando resumen...</StatusMessage>
      ) : (
        <div className="grid gap-4 md:grid-cols-3">
          <DataCard title="Préstamos activos" value={activeLoans} tone="lavender" icon="📚">
            Máximo permitido: 3 activos.
          </DataCard>
          <DataCard title="Reservas activas" value={activeReservations} tone="peach" icon="⏰">
            Revisa las asignadas: tienes 48h para retirarlas.
          </DataCard>
          <DataCard
            title="Multas pendientes"
            value={formatMoney(pendingAmount)}
            tone={pendingFines.length ? 'sky' : 'mint'}
            icon="💳"
          >
            {pendingFines.length} multa(s) por regularizar.
          </DataCard>
        </div>
      )}
    </section>
  );
}
