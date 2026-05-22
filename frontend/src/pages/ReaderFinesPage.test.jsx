import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import ReaderFinesPage from './ReaderFinesPage.jsx';
import * as circulationService from '../services/circulationService.js';

vi.mock('../services/circulationService.js');

describe('ReaderFinesPage', () => {
  it('simulates a successful payment and shows the reference', async () => {
    circulationService.fetchFines
      .mockResolvedValueOnce([{ id: 1, prestamoId: 9, diasRetraso: 2, monto: 100, estado: 'Pendiente', intentosPago: [] }])
      .mockResolvedValueOnce([{ id: 1, prestamoId: 9, diasRetraso: 2, monto: 100, estado: 'Pagada', fechaPago: '2026-05-17T00:00:00Z', pago: { recibo: 'REC-SIM-1' }, intentosPago: [{ referencia: 'SIM-1' }] }]);
    circulationService.payFine.mockResolvedValue({ referencia: 'SIM-1' });

    render(<ReaderFinesPage />);

    await userEvent.click(await screen.findByRole('button', { name: 'Aprobar pago demo' }));

    expect(circulationService.payFine).toHaveBeenCalledWith(1, true);
    await waitFor(() => expect(screen.getByText('Pago simulado aprobado. Referencia SIM-1.')).toBeInTheDocument());
  });
});
