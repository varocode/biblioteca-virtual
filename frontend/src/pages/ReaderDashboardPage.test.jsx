import { render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import ReaderDashboardPage from './ReaderDashboardPage.jsx';

vi.mock('../services/circulationService.js', () => ({
  fetchLoans: () => Promise.resolve([{ id: 1, estado: 1 }]),
  fetchReservations: () => Promise.resolve([{ id: 2, estado: 2 }]),
  fetchFines: () => Promise.resolve([{ id: 3, estado: 1, monto: 100 }])
}));

describe('ReaderDashboardPage', () => {
  it('shows reader circulation summary', async () => {
    render(<ReaderDashboardPage />);
    await waitFor(() => expect(screen.getByText('Préstamos activos')).toBeInTheDocument());
    expect(screen.getByText('RD$100.00')).toBeInTheDocument();
  });
});
