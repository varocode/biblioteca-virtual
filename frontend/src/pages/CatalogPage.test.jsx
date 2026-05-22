import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import CatalogPage from './CatalogPage.jsx';
import * as catalogService from '../services/catalogService.js';
import * as circulationService from '../services/circulationService.js';

const authState = vi.hoisted(() => ({ value: { isAuthenticated: true } }));

vi.mock('../services/catalogService.js');
vi.mock('../services/circulationService.js');
vi.mock('../context/AuthContext.jsx', () => ({
  useAuth: () => authState.value
}));

describe('CatalogPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    authState.value = { isAuthenticated: true };
    catalogService.fetchAuthors.mockResolvedValue([{ id: 1, nombre: 'Isabel Allende' }]);
    catalogService.fetchCategories.mockResolvedValue([{ id: 2, nombre: 'Novela' }]);
    circulationService.createLoan.mockResolvedValue({});
    circulationService.createReservation.mockResolvedValue({});
  });

  it('renders public catalog results with availability', async () => {
    catalogService.fetchBooks.mockResolvedValue({
      items: [{ id: 10, titulo: 'La casa de los espíritus', autor: { nombre: 'Isabel Allende' }, categoria: { nombre: 'Novela' }, editorial: 'Demo Press', anio: 1982, disponibles: 2, ejemplaresDisponibles: 2, stock: 3, formatos: ['Fisico'], ubicaciones: ['Estante 1'], etiquetaDisponibilidad: '2 ejemplares disponibles' }],
      total: 1,
      page: 1,
      pageSize: 9
    });

    render(<MemoryRouter><CatalogPage /></MemoryRouter>);

    expect(screen.getByText('Cargando catálogo...')).toBeInTheDocument();
    await waitFor(() => expect(screen.getByText('La casa de los espíritus')).toBeInTheDocument());
    expect(screen.getByText('2 ejemplares disponibles')).toBeInTheDocument();
    expect(screen.getByText('Demo Press · 1982')).toBeInTheDocument();
    expect(screen.getByText('Fisico · Estante 1')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Solicitar préstamo' })).toBeInTheDocument();
  });

  it('sends availability, digital format and sort filters to the API', async () => {
    catalogService.fetchBooks.mockResolvedValue({
      items: [{ id: 12, titulo: 'Manual digital', autor: { nombre: 'Autor' }, categoria: { nombre: 'Tecnología' }, disponibles: 1, ejemplaresDisponibles: 1, stock: 1, formatos: ['Digital'], ubicaciones: ['Biblioteca digital'], etiquetaDisponibilidad: '1 ejemplar disponible' }],
      total: 1,
      page: 1,
      pageSize: 9
    });

    render(<MemoryRouter><CatalogPage /></MemoryRouter>);

    await screen.findByText('Manual digital');
    await userEvent.selectOptions(screen.getByLabelText('Filtrar por disponibilidad'), 'true');
    await userEvent.selectOptions(screen.getByLabelText('Filtrar por formato'), 'Digital');
    await userEvent.selectOptions(screen.getByLabelText('Ordenar catálogo'), 'disponibilidad:desc');

    await waitFor(() => expect(catalogService.fetchBooks).toHaveBeenLastCalledWith(expect.objectContaining({ disponible: 'true', tipoEjemplar: 'Digital', sortBy: 'disponibilidad', sortDir: 'desc' })));
  });

  it('lets authenticated readers request an available book from the catalog', async () => {
    catalogService.fetchBooks.mockResolvedValue({
      items: [{ id: 10, titulo: 'La casa de los espíritus', autor: { nombre: 'Isabel Allende' }, categoria: { nombre: 'Novela' }, disponibles: 2, stock: 3 }],
      total: 1,
      page: 1,
      pageSize: 9
    });

    render(<MemoryRouter><CatalogPage /></MemoryRouter>);

    await userEvent.click(await screen.findByRole('button', { name: 'Solicitar préstamo' }));

    expect(circulationService.createLoan).toHaveBeenCalledWith(10);
    await waitFor(() => expect(screen.getByText('Préstamo solicitado.')).toBeInTheDocument());
  });

  it('lets authenticated readers reserve an unavailable book from the catalog', async () => {
    catalogService.fetchBooks.mockResolvedValue({
      items: [{ id: 11, titulo: 'Libro reservado', autor: { nombre: 'Autor' }, categoria: { nombre: 'Novela' }, disponibles: 0, ejemplaresDisponibles: 0, stock: 1, etiquetaDisponibilidad: 'No disponible — puedes reservarlo' }],
      total: 1,
      page: 1,
      pageSize: 9
    });

    render(<MemoryRouter><CatalogPage /></MemoryRouter>);

    await userEvent.click(await screen.findByRole('button', { name: 'Reservar' }));

    expect(circulationService.createReservation).toHaveBeenCalledWith(11);
    expect(screen.getByText('No disponible — puedes reservarlo')).toBeInTheDocument();
    await waitFor(() => expect(screen.getByText('Reserva creada.')).toBeInTheDocument());
  });

  it('shows an empty state when filters have no matches', async () => {
    catalogService.fetchBooks.mockResolvedValue({ items: [], total: 0, page: 1, pageSize: 9 });

    render(<MemoryRouter><CatalogPage /></MemoryRouter>);

    await waitFor(() => expect(screen.getByText('No hay libros para esos filtros.')).toBeInTheDocument());
  });
});
