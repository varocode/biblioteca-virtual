import { Link } from 'react-router-dom';
import BookActions from './BookActions.jsx';
import BookCover from './BookCover.jsx';

export default function BookCard({ book, onActionDone }) {
  const availableCopies = book.ejemplaresDisponibles ?? book.disponibles;
  const isAvailable = availableCopies > 0;
  const formats = book.formatos?.length ? book.formatos.join(' + ') : 'Formato sin registrar';
  const locations = book.ubicaciones?.length ? book.ubicaciones.slice(0, 2).join(', ') : 'Ubicación sin registrar';

  return (
    <article className="group relative flex h-full flex-col overflow-hidden rounded-3xl border border-white/70 bg-white/85 shadow-soft backdrop-blur-sm transition hover:-translate-y-1 hover:shadow-glow">
      <div className="relative">
        <BookCover book={book} size="L" rounded="rounded-none" className="aspect-[3/4] w-full" />
        <span
          className={`absolute left-3 top-3 inline-flex items-center gap-1 rounded-full px-3 py-1 text-[11px] font-semibold backdrop-blur ${
            isAvailable
              ? 'bg-mint-100/90 text-mint-700'
              : 'bg-peach-100/90 text-peach-700'
          }`}
        >
          <span className={`h-1.5 w-1.5 rounded-full ${isAvailable ? 'bg-mint-500' : 'bg-peach-500'}`} />
          {isAvailable ? 'Disponible' : 'Reservable'}
        </span>
        {book.categoria?.nombre && (
          <span className="absolute right-3 top-3 rounded-full bg-white/80 px-3 py-1 text-[11px] font-semibold text-ink-700 backdrop-blur">
            {book.categoria.nombre}
          </span>
        )}
      </div>

      <div className="flex flex-1 flex-col gap-2 p-5">
        <h3 className="font-display text-lg font-bold leading-snug text-ink-900 line-clamp-2">{book.titulo}</h3>
        <p className="text-sm font-medium text-lavender-700">{book.autor?.nombre || 'Autor sin registrar'}</p>
        <p className="text-xs text-ink-500">
          {book.editorial || 'Editorial sin registrar'} · {book.anio}
        </p>
        <p className="text-xs text-ink-500">{formats} · {locations}</p>
        <p className="mt-1 line-clamp-3 text-sm text-ink-700/80">
          {book.sinopsis || 'Sin sinopsis disponible.'}
        </p>

        <div className="mt-auto flex items-center justify-between pt-4">
          <span className="text-xs font-semibold text-ink-500">
            {book.etiquetaDisponibilidad || (isAvailable ? `${availableCopies} ejemplares` : 'Sin stock')}
          </span>
          <Link
            className="text-sm font-semibold text-lavender-700 hover:text-lavender-500"
            to={`/catalogo/${book.id}`}
          >
            Ver detalle →
          </Link>
        </div>

        <div className="mt-3 border-t border-lavender-100/70 pt-3">
          <BookActions book={book} onDone={onActionDone} compact />
        </div>
      </div>
    </article>
  );
}
