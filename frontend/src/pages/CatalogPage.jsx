import { useCallback, useEffect, useState } from 'react';
import BookCard from '../components/BookCard.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { fetchAuthors, fetchBooks, fetchCategories } from '../services/catalogService.js';
import { getErrorMessage } from '../services/api.js';

const initialFilters = {
  search: '',
  autorId: '',
  categoriaId: '',
  disponible: '',
  tipoEjemplar: '',
  page: 1,
  pageSize: 9,
  sortBy: 'titulo',
  sortDir: 'asc'
};

export default function CatalogPage() {
  const [filters, setFilters] = useState(initialFilters);
  const [books, setBooks] = useState({ items: [], total: 0, page: 1, pageSize: 9 });
  const [lookups, setLookups] = useState({ authors: [], categories: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadBooks = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      setBooks(await fetchBooks(filters));
    } catch (err) {
      setError(getErrorMessage(err, 'No se pudo cargar el catálogo.'));
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    Promise.all([fetchAuthors(), fetchCategories()])
      .then(([authors, categories]) => setLookups({ authors, categories }))
      .catch(() => setLookups({ authors: [], categories: [] }));
  }, []);

  useEffect(() => {
    loadBooks();
  }, [loadBooks]);

  const totalPages = Math.max(1, Math.ceil(books.total / books.pageSize));

  function updateFilter(name, value) {
    setFilters((current) => ({ ...current, [name]: value, page: 1 }));
  }

  return (
    <section className="space-y-8">
      <div className="relative overflow-hidden rounded-[2rem] border border-white/60 bg-pastel-hero p-8 shadow-soft md:p-10">
        <div className="absolute -right-16 -top-16 h-56 w-56 rounded-full bg-white/40 blur-3xl" aria-hidden />
        <div className="absolute -bottom-20 -left-12 h-56 w-56 rounded-full bg-lavender-200/60 blur-3xl" aria-hidden />
        <div className="relative max-w-2xl">
          <p className="inline-flex items-center gap-2 rounded-full bg-white/70 px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] text-lavender-700 backdrop-blur">
            <span className="h-1.5 w-1.5 rounded-full bg-lavender-500" />
            Catálogo Unicaribe
          </p>
          <h1 className="mt-4 font-display text-4xl font-extrabold tracking-tight text-ink-900 md:text-5xl">
            Explora libros disponibles
          </h1>
          <p className="mt-3 text-base text-ink-700/80">
            Busca por título, autor, categoría, ISBN, editorial, código de ejemplar o ubicación.
            Filtra por disponibilidad y formato.
          </p>
        </div>
      </div>

      <form
        className="grid gap-3 rounded-3xl border border-white/70 bg-white/85 p-5 shadow-soft backdrop-blur-sm md:grid-cols-6"
        onSubmit={(event) => event.preventDefault()}
      >
        <input
          aria-label="Buscar catálogo"
          className="input-pastel md:col-span-2"
          placeholder="🔍 Buscar por título, autor, ISBN…"
          value={filters.search}
          onChange={(e) => updateFilter('search', e.target.value)}
        />
        <select
          aria-label="Filtrar por autor"
          className="input-pastel"
          value={filters.autorId}
          onChange={(e) => updateFilter('autorId', e.target.value)}
        >
          <option value="">Autores</option>
          {lookups.authors.map((author) => (
            <option key={author.id} value={author.id}>{author.nombre}</option>
          ))}
        </select>
        <select
          aria-label="Filtrar por categoría"
          className="input-pastel"
          value={filters.categoriaId}
          onChange={(e) => updateFilter('categoriaId', e.target.value)}
        >
          <option value="">Categorías</option>
          {lookups.categories.map((category) => (
            <option key={category.id} value={category.id}>{category.nombre}</option>
          ))}
        </select>
        <select
          aria-label="Filtrar por disponibilidad"
          className="input-pastel"
          value={filters.disponible}
          onChange={(e) => updateFilter('disponible', e.target.value)}
        >
          <option value="">Todos</option>
          <option value="true">Disponibles</option>
          <option value="false">No disponibles</option>
        </select>
        <select
          aria-label="Filtrar por formato"
          className="input-pastel"
          value={filters.tipoEjemplar}
          onChange={(e) => updateFilter('tipoEjemplar', e.target.value)}
        >
          <option value="">Todos los formatos</option>
          <option value="Fisico">Físicos</option>
          <option value="Digital">Digitales</option>
        </select>
        <select
          aria-label="Ordenar catálogo"
          className="input-pastel md:col-span-6"
          value={`${filters.sortBy}:${filters.sortDir}`}
          onChange={(e) => {
            const [sortBy, sortDir] = e.target.value.split(':');
            setFilters((current) => ({ ...current, sortBy, sortDir, page: 1 }));
          }}
        >
          <option value="titulo:asc">Ordenar: Título A-Z</option>
          <option value="recientes:desc">Ordenar: Más recientes</option>
          <option value="anio:desc">Ordenar: Año descendente</option>
          <option value="disponibilidad:desc">Ordenar: Más disponibilidad</option>
        </select>
      </form>

      {error && <StatusMessage type="error">{error}</StatusMessage>}
      {loading && <StatusMessage>Cargando catálogo...</StatusMessage>}
      {!loading && !error && books.items.length === 0 && (
        <StatusMessage>No hay libros para esos filtros.</StatusMessage>
      )}

      <div className="grid gap-5 md:grid-cols-2 lg:grid-cols-3">
        {books.items.map((book) => (
          <BookCard key={book.id} book={book} onActionDone={loadBooks} />
        ))}
      </div>

      {!loading && books.total > 0 && (
        <div className="flex items-center justify-between rounded-3xl border border-white/70 bg-white/85 px-5 py-4 shadow-soft backdrop-blur-sm">
          <span className="text-sm text-ink-700">
            Página <strong className="text-ink-900">{books.page}</strong> de {totalPages} ·{' '}
            <span className="text-ink-500">{books.total} resultados</span>
          </span>
          <div className="flex gap-2">
            <button
              className="rounded-full border border-lavender-200 bg-white px-4 py-2 text-sm font-semibold text-ink-700 transition hover:bg-lavender-50 disabled:cursor-not-allowed disabled:opacity-50"
              disabled={filters.page <= 1}
              onClick={() => setFilters((current) => ({ ...current, page: current.page - 1 }))}
            >
              ← Anterior
            </button>
            <button
              className="rounded-full bg-lavender-500 px-4 py-2 text-sm font-semibold text-white shadow-glow transition hover:bg-lavender-600 disabled:cursor-not-allowed disabled:opacity-50"
              disabled={filters.page >= totalPages}
              onClick={() => setFilters((current) => ({ ...current, page: current.page + 1 }))}
            >
              Siguiente →
            </button>
          </div>
        </div>
      )}
    </section>
  );
}
