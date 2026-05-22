import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import AdminAuditPage from './AdminAuditPage.jsx';
import * as adminService from '../services/adminService.js';

vi.mock('../services/adminService.js');

describe('AdminAuditPage', () => {
  it('renders append-only audit events', async () => {
    adminService.fetchAudit.mockResolvedValue([{ id: 1, actorNombre: 'Admin', accion: 'pago.aprobar', entidad: 'Multa', entidadId: '5', resultado: 'Pago simulado aprobado.', fecha: '2026-05-17T00:00:00Z' }]);

    render(<MemoryRouter><AdminAuditPage /></MemoryRouter>);

    expect(await screen.findByText('pago.aprobar')).toBeInTheDocument();
    expect(screen.getByText('Multa #5')).toBeInTheDocument();
  });
});
