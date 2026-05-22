import { useEffect, useMemo, useState } from 'react';

const GRADIENTS = [
  'from-lavender-200 via-lavender-100 to-peach-100',
  'from-peach-200 via-peach-100 to-sun-100',
  'from-mint-200 via-mint-100 to-sky-100',
  'from-sky-200 via-sky-100 to-lavender-100',
  'from-sun-200 via-peach-100 to-lavender-100',
  'from-lavender-200 via-sky-100 to-mint-100'
];

const CATEGORY_KEYWORDS = {
  novela: 'novel,book,literature',
  literatura: 'novel,book,literature',
  ficción: 'fiction,story,book',
  ficcion: 'fiction,story,book',
  'no ficción': 'nonfiction,book,reading',
  'no ficcion': 'nonfiction,book,reading',
  ciencia: 'science,laboratory,research',
  tecnología: 'technology,computer,code',
  tecnologia: 'technology,computer,code',
  historia: 'history,ancient,archive',
  arte: 'art,gallery,museum',
  filosofía: 'philosophy,thinking,book',
  filosofia: 'philosophy,thinking,book',
  poesía: 'poetry,verse,writing',
  poesia: 'poetry,verse,writing',
  infantil: 'children,kids,picture',
  educación: 'education,school,classroom',
  educacion: 'education,school,classroom',
  matemáticas: 'mathematics,equation,blackboard',
  matematicas: 'mathematics,equation,blackboard'
};

const BOOK_KEYWORDS = 'book,library,reading';

function pickGradient(seed = '') {
  let hash = 0;
  for (let i = 0; i < seed.length; i += 1) {
    hash = (hash * 31 + seed.charCodeAt(i)) >>> 0;
  }
  return GRADIENTS[hash % GRADIENTS.length];
}

function hashCode(value = '') {
  let hash = 0;
  for (let i = 0; i < value.length; i += 1) {
    hash = (hash * 33 + value.charCodeAt(i)) >>> 0;
  }
  return hash;
}

function initials(title = '') {
  return (
    title
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((word) => word[0]?.toUpperCase())
      .join('') || 'L'
  );
}

function sanitizeIsbn(isbn) {
  if (!isbn) return '';
  return String(isbn).replace(/[^0-9Xx]/g, '');
}

function categoryQuery(book) {
  const name = (book?.categoria?.nombre || '').toLowerCase().trim();
  return CATEGORY_KEYWORDS[name] || BOOK_KEYWORDS;
}

export default function BookCover({ book, size = 'M', className = '', rounded = 'rounded-2xl' }) {
  const isbn = sanitizeIsbn(book?.isbn);
  const sources = useMemo(() => {
    const list = [];
    if (book?.portadaUrl) list.push(book.portadaUrl);
    if (isbn) list.push(`https://covers.openlibrary.org/b/isbn/${isbn}-${size}.jpg?default=false`);
    const seed = hashCode(`${book?.id ?? ''}-${book?.titulo ?? ''}`);
    const query = categoryQuery(book);
    list.push(`https://loremflickr.com/600/800/${encodeURIComponent(query)}?lock=${seed}`);
    list.push(`https://picsum.photos/seed/${encodeURIComponent(book?.titulo || book?.id || 'libro')}/600/800`);
    return list;
  }, [book?.portadaUrl, book?.id, book?.titulo, book?.categoria?.nombre, isbn, size]);

  const [index, setIndex] = useState(0);
  useEffect(() => {
    setIndex(0);
  }, [sources.join('|')]);

  const gradient = pickGradient(book?.titulo || isbn);
  const showImage = index < sources.length;

  return (
    <div
      className={`relative overflow-hidden bg-gradient-to-br ${gradient} ${rounded} ${className}`}
      aria-hidden={showImage ? undefined : true}
    >
      <div className="absolute inset-0 flex flex-col items-center justify-center gap-1 px-3 text-center">
        <span className="font-display text-3xl font-bold text-ink-900/70 drop-shadow-sm">
          {initials(book?.titulo)}
        </span>
        {book?.autor?.nombre && (
          <span className="line-clamp-2 text-[10px] font-medium uppercase tracking-wider text-ink-700/70">
            {book.autor.nombre}
          </span>
        )}
      </div>
      {showImage && (
        <img
          src={sources[index]}
          alt={book?.titulo ? `Portada de ${book.titulo}` : ''}
          loading="lazy"
          className="relative h-full w-full object-cover"
          onError={() => setIndex((current) => current + 1)}
        />
      )}
      <div className="pointer-events-none absolute inset-x-0 bottom-0 h-12 bg-gradient-to-t from-black/30 via-black/10 to-transparent" />
    </div>
  );
}
