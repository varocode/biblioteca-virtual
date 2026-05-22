import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import AdminDashboardPage from './AdminDashboardPage.jsx';

vi.mock('../services/adminService.js', () => ({
  fetchDashboard: () => Promise.resolve({
    totalLibros: 8,
    totalUsuarios: 4,
    prestamosActivos: 2,
    prestamosVencidos: 1,
    reservasActivas: 3,
    multasPendientes: 1,
    montoMultasPendientes: 50,
    topLibros: [{ etiqueta: 'Clean Code', valor: 5 }],
    usuariosActivos: [],
    prestamosPorMes: [],
    categoriasPopulares: []
  })
}));

describe('AdminDashboardPage', () => {
  it('renders admin metrics and chart data', async () => {
    render(<MemoryRouter><AdminDashboardPage /></MemoryRouter>);
    await waitFor(() => expect(screen.getByRole('heading', { name: 'Dashboard' })).toBeInTheDocument());
    expect(screen.getByText('Clean Code')).toBeInTheDocument();
    expect(screen.getByText('RD$50.00')).toBeInTheDocument();
  });
});
