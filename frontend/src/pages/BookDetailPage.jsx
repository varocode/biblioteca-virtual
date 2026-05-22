import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import StatusMessage from '../components/StatusMessage.jsx';
import BookActions from '../components/BookActions.jsx';
import BookCover from '../components/BookCover.jsx';
import { getErrorMessage } from '../services/api.js';
import { fetchBook } from '../services/catalogService.js';

export default function BookDetailPage() {
  const { id } = useParams();
  const [book, setBook] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  async function loadBook() {
    setLoading(true);
    setError('');
    try {
      setBook(await fetchBook(id));
    } catch (err) {
      setError(getErrorMessage(err, 'No se pudo cargar el libro.'));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadBook();
  }, [id]);

  if (loading) return <StatusMessage>Cargando detalle...</StatusMessage>;
  if (error) return <StatusMessage type="error">{error}</StatusMessage>;
  if (!book) return <StatusMessage>No encontramos el libro.</StatusMessage>;
  const availableCopies = book.ejemplaresDisponibles ?? book.disponibles;
  const isAvailable = availableCopies > 0;

  return (
    <article className="overflow-hidden rounded-[2rem] border border-white/70 bg-white/85 p-6 shadow-soft backdrop-blur-sm md:p-10">
      <Link
        to="/catalogo"
        className="inline-flex items-center gap-2 text-sm font-semibold text-lavender-700 hover:text-lavender-500"
      >
        ← Volver al catálogo
      </Link>

      <div className="mt-8 grid gap-8 md:grid-cols-[260px_1fr]">
        <div className="space-y-4">
          <BookCover book={book} size="L" className="aspect-[3/4] w-full shadow-glow" rounded="rounded-3xl" />
          <span
            className={`inline-flex w-full items-center justify-center rounded-full px-4 py-2 text-sm font-semibold ${
              isAvailable ? 'bg-mint-100 text-mint-700' : 'bg-peach-100 text-peach-700'
            }`}
          >
            {book.etiquetaDisponibilidad || `${availableCopies} de ${book.stock} ejemplares`}
          </span>
        </div>

        <div className="space-y-4">
          <div>
            <p className="inline-flex items-center gap-2 rounded-full bg-lavender-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] text-lavender-700">
              {book.categoria?.nombre || 'Sin categoría'}
            </p>
            <h1 className="mt-3 font-display text-4xl font-extrabold tracking-tight text-ink-900">
              {book.titulo}
            </h1>
            <p className="mt-2 text-lg font-medium text-lavender-700">{book.autor?.nombre}</p>
          </div>

          <div className="grid grid-cols-2 gap-3 text-sm text-ink-700 sm:grid-cols-3">
            <div className="rounded-2xl bg-cream-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-wider text-ink-500">Año</p>
              <p className="mt-1 font-semibold">{book.anio || '—'}</p>
            </div>
            <div className="rounded-2xl bg-cream-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-wider text-ink-500">ISBN</p>
              <p className="mt-1 font-semibold">{book.isbn || '—'}</p>
            </div>
            <div className="rounded-2xl bg-cream-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-wider text-ink-500">Editorial</p>
              <p className="mt-1 font-semibold">{book.editorial || '—'}</p>
            </div>
            <div className="rounded-2xl bg-cream-50 px-4 py-3 sm:col-span-2">
              <p className="text-[11px] font-semibold uppercase tracking-wider text-ink-500">Formato</p>
              <p className="mt-1 font-semibold">{book.formatos?.join(' + ') || 'Sin registrar'}</p>
            </div>
            <div className="rounded-2xl bg-cream-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-wider text-ink-500">Ubicación</p>
              <p className="mt-1 font-semibold">{book.ubicaciones?.join(', ') || 'Sin registrar'}</p>
            </div>
          </div>

          <p className="text-base leading-relaxed text-ink-700">
            {book.sinopsis || 'Sin sinopsis disponible.'}
          </p>

          {book.ejemplares?.length > 0 && (
            <div className="rounded-2xl border border-lavender-100 bg-lavender-50/60 p-4">
              <h2 className="text-sm font-semibold uppercase tracking-wider text-lavender-700">Ejemplares</h2>
              <ul className="mt-3 grid gap-2 text-sm text-ink-700 sm:grid-cols-2">
                {book.ejemplares.map((copy) => (
                  <li
                    key={copy.id}
                    className="rounded-xl bg-white px-3 py-2 shadow-sm ring-1 ring-lavender-100/60"
                  >
                    <span className="font-semibold text-ink-900">{copy.codigo}</span> · {copy.tipo} · {copy.estado}
                    {copy.ubicacion ? <span className="text-ink-500"> · {copy.ubicacion}</span> : null}
                  </li>
                ))}
              </ul>
            </div>
          )}

          <BookActions book={book} onDone={loadBook} />
        </div>
      </div>
    </article>
  );
}
