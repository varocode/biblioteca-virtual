import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import AdminCatalogPage from './AdminCatalogPage.jsx';
import * as adminService from '../services/adminService.js';
import * as catalogService from '../services/catalogService.js';

vi.mock('../services/adminService.js');
vi.mock('../services/catalogService.js');

describe('AdminCatalogPage', () => {
  beforeEach(() => {
    catalogService.fetchAuthors.mockResolvedValue([{ id: 1, nombre: 'Autor Demo', nacionalidad: 'DO' }]);
    catalogService.fetchCategories.mockResolvedValue([{ id: 2, nombre: 'Ficción', descripcion: 'Novelas' }]);
    catalogService.fetchBooks.mockResolvedValue({
      items: [{ id: 10, titulo: 'Libro Demo', isbn: '978-demo', anio: 2024, stock: 3, disponibles: 2, autor: { id: 1, nombre: 'Autor Demo' }, categoria: { id: 2, nombre: 'Ficción' } }],
      total: 1,
      page: 1,
      pageSize: 50
    });
    adminService.saveBook.mockResolvedValue({});
    adminService.saveAuthor.mockResolvedValue({});
    adminService.saveCategory.mockResolvedValue({});
    adminService.deleteBook.mockResolvedValue();
    adminService.deleteAuthor.mockResolvedValue();
    adminService.deleteCategory.mockResolvedValue();
    vi.spyOn(window, 'confirm').mockReturnValue(true);
  });

  it('allows admins to edit an existing book through the CRUD form', async () => {
    const user = userEvent.setup();
    render(<MemoryRouter><AdminCatalogPage /></MemoryRouter>);

    await waitFor(() => expect(screen.getByText('Libro Demo')).toBeInTheDocument());
    await user.click(screen.getAllByRole('button', { name: 'Editar' })[0]);
    expect(screen.getByRole('heading', { name: 'Editar libro' })).toBeInTheDocument();

    await user.clear(screen.getByLabelText('Título'));
    await user.type(screen.getByLabelText('Título'), 'Libro Demo Actualizado');
    await user.click(screen.getByRole('button', { name: 'Actualizar libro' }));

    await waitFor(() => expect(adminService.saveBook).toHaveBeenCalledWith(expect.objectContaining({ titulo: 'Libro Demo Actualizado' }), 10));
    expect(await screen.findByText('Libro actualizado.')).toBeInTheDocument();
  });
});
