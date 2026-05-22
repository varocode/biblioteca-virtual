import { useEffect, useState } from 'react';
import AdminTabs from '../components/AdminTabs.jsx';
import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { getErrorMessage } from '../services/api.js';
import { deleteAuthor, deleteBook, deleteCategory, saveAuthor, saveBook, saveCategory } from '../services/adminService.js';
import { fetchAuthors, fetchBooks, fetchCategories } from '../services/catalogService.js';

const currentYear = new Date().getFullYear();
const emptyBook = { titulo: '', isbn: '', anio: currentYear, editorial: '', sinopsis: '', portadaUrl: '', stock: 1, disponibles: 1, autorId: '', categoriaId: '' };
const emptyAuthor = { nombre: '', nacionalidad: '', biografia: '' };
const emptyCategory = { nombre: '', descripcion: '' };

const primaryBtn = 'rounded-full bg-gradient-to-r from-lavender-500 to-lavender-600 px-4 py-2 text-sm font-semibold text-white shadow-glow transition hover:from-lavender-600 hover:to-lavender-700 disabled:cursor-not-allowed disabled:opacity-50';
const ghostBtn = 'rounded-full border border-lavender-200 bg-white px-4 py-2 text-sm font-semibold text-ink-900 transition hover:border-lavender-400 hover:bg-lavender-50';
const dangerBtn = 'rounded-full border border-peach-200 bg-white px-3 py-1.5 text-xs font-semibold text-ink-900 transition hover:border-peach-400 hover:bg-peach-100 hover:text-peach-700';
const editBtn = 'rounded-full border border-lavender-200 bg-white px-3 py-1.5 text-xs font-semibold text-ink-900 transition hover:border-lavender-400 hover:bg-lavender-50';

export default function AdminCatalogPage() {
  const [data, setData] = useState({ books: [], authors: [], categories: [] });
  const [book, setBook] = useState(emptyBook);
  const [author, setAuthor] = useState(emptyAuthor);
  const [category, setCategory] = useState(emptyCategory);
  const [editing, setEditing] = useState({ bookId: null, authorId: null, categoryId: null });
  const [status, setStatus] = useState({ loading: true, saving: false, error: '', success: '' });

  const load = () =>
    Promise.all([fetchBooks({ pageSize: 50 }), fetchAuthors(), fetchCategories()])
      .then(([books, authors, categories]) => setData({ books: books.items ?? [], authors, categories }))
      .catch((err) => setStatus((s) => ({ ...s, error: getErrorMessage(err, 'No se pudo cargar el catálogo admin.') })))
      .finally(() => setStatus((s) => ({ ...s, loading: false })));

  useEffect(() => {
    load();
  }, []);

  async function run(work, success, reset) {
    setStatus((s) => ({ ...s, saving: true, error: '', success: '' }));
    try {
      await work();
      await load();
      reset?.();
      setStatus({ loading: false, saving: false, error: '', success });
    } catch (err) {
      setStatus({ loading: false, saving: false, error: getErrorMessage(err), success: '' });
    }
  }

  const resetBook = () => {
    setBook(emptyBook);
    setEditing((current) => ({ ...current, bookId: null }));
  };
  const resetAuthor = () => {
    setAuthor(emptyAuthor);
    setEditing((current) => ({ ...current, authorId: null }));
  };
  const resetCategory = () => {
    setCategory(emptyCategory);
    setEditing((current) => ({ ...current, categoryId: null }));
  };

  return (
    <section className="space-y-6">
      <AdminTabs />
      <PageHeader eyebrow="Administración" title="Catálogo CRUD">
        Crea, edita y elimina libros, autores y categorías con validación del API y confirmaciones destructivas.
      </PageHeader>

      {status.error && <StatusMessage type="error">{status.error}</StatusMessage>}
      {status.success && <StatusMessage type="success">{status.success}</StatusMessage>}

      {status.loading ? (
        <StatusMessage>Cargando catálogo...</StatusMessage>
      ) : (
        <div className="grid gap-6 lg:grid-cols-2">
          <BookForm
            values={book}
            setValues={setBook}
            authors={data.authors}
            categories={data.categories}
            editing={Boolean(editing.bookId)}
            saving={status.saving}
            onCancel={resetBook}
            onSave={() => run(() => saveBook(book, editing.bookId), editing.bookId ? 'Libro actualizado.' : 'Libro guardado.', resetBook)}
          />
          <div className="space-y-6">
            <LookupForm
              title={editing.authorId ? 'Editar autor' : 'Nuevo autor'}
              values={author}
              setValues={setAuthor}
              saving={status.saving}
              onCancel={resetAuthor}
              onSave={() => run(() => saveAuthor(author, editing.authorId), editing.authorId ? 'Autor actualizado.' : 'Autor guardado.', resetAuthor)}
            />
            <LookupForm
              title={editing.categoryId ? 'Editar categoría' : 'Nueva categoría'}
              values={category}
              setValues={setCategory}
              saving={status.saving}
              category
              onCancel={resetCategory}
              onSave={() => run(() => saveCategory(category, editing.categoryId), editing.categoryId ? 'Categoría actualizada.' : 'Categoría guardada.', resetCategory)}
            />
          </div>
          <List
            title="Libros"
            items={data.books}
            onEdit={(item) => {
              setBook({ ...emptyBook, ...item, autorId: item.autor?.id ?? '', categoriaId: item.categoria?.id ?? '' });
              setEditing((current) => ({ ...current, bookId: item.id }));
            }}
            onDelete={(item) => window.confirm(`¿Eliminar ${item.titulo}?`) && run(() => deleteBook(item.id), 'Libro eliminado.', resetBook)}
          />
          <List
            title="Autores"
            items={data.authors}
            label="nombre"
            onEdit={(item) => {
              setAuthor({ ...emptyAuthor, ...item });
              setEditing((current) => ({ ...current, authorId: item.id }));
            }}
            onDelete={(item) => window.confirm(`¿Eliminar ${item.nombre}?`) && run(() => deleteAuthor(item.id), 'Autor eliminado.', resetAuthor)}
          />
          <List
            title="Categorías"
            items={data.categories}
            label="nombre"
            onEdit={(item) => {
              setCategory({ ...emptyCategory, ...item });
              setEditing((current) => ({ ...current, categoryId: item.id }));
            }}
            onDelete={(item) => window.confirm(`¿Eliminar ${item.nombre}?`) && run(() => deleteCategory(item.id), 'Categoría eliminada.', resetCategory)}
          />
        </div>
      )}
    </section>
  );
}

function BookForm({ values, setValues, authors, categories, editing, saving, onSave, onCancel }) {
  return (
    <form
      className="space-y-3 rounded-3xl border border-white/70 bg-white/85 p-5 shadow-soft backdrop-blur-sm"
      onSubmit={(event) => {
        event.preventDefault();
        onSave();
      }}
    >
      <h2 className="font-display text-lg font-bold text-ink-900">{editing ? 'Editar libro' : 'Nuevo libro'}</h2>
      <input required className="input-pastel w-full" aria-label="Título" placeholder="Título" value={values.titulo} onChange={(event) => setValues({ ...values, titulo: event.target.value })} />
      <input required className="input-pastel w-full" aria-label="ISBN" placeholder="ISBN" value={values.isbn} onChange={(event) => setValues({ ...values, isbn: event.target.value })} />
      <div className="grid gap-3 md:grid-cols-3">
        <input required type="number" className="input-pastel" aria-label="Año" value={values.anio} onChange={(event) => setValues({ ...values, anio: event.target.value })} />
        <input required type="number" min="0" className="input-pastel" aria-label="Stock" value={values.stock} onChange={(event) => setValues({ ...values, stock: event.target.value })} />
        <input required type="number" min="0" className="input-pastel" aria-label="Disponibles" value={values.disponibles} onChange={(event) => setValues({ ...values, disponibles: event.target.value })} />
      </div>
      <select required className="input-pastel w-full" aria-label="Autor" value={values.autorId} onChange={(event) => setValues({ ...values, autorId: event.target.value })}>
        <option value="">Autor</option>
        {authors.map((author) => (
          <option key={author.id} value={author.id}>
            {author.nombre}
          </option>
        ))}
      </select>
      <select required className="input-pastel w-full" aria-label="Categoría" value={values.categoriaId} onChange={(event) => setValues({ ...values, categoriaId: event.target.value })}>
        <option value="">Categoría</option>
        {categories.map((category) => (
          <option key={category.id} value={category.id}>
            {category.nombre}
          </option>
        ))}
      </select>
      <input className="input-pastel w-full" aria-label="Editorial" placeholder="Editorial" value={values.editorial ?? ''} onChange={(event) => setValues({ ...values, editorial: event.target.value })} />
      <textarea className="input-pastel w-full" rows={3} aria-label="Sinopsis" placeholder="Sinopsis" value={values.sinopsis ?? ''} onChange={(event) => setValues({ ...values, sinopsis: event.target.value })} />
      <div className="flex flex-wrap gap-2">
        <button disabled={saving} className={primaryBtn}>
          {editing ? 'Actualizar libro' : 'Guardar libro'}
        </button>
        {editing && (
          <button type="button" className={ghostBtn} onClick={onCancel}>
            Cancelar edición
          </button>
        )}
      </div>
    </form>
  );
}

function LookupForm({ title, values, setValues, onSave, onCancel, saving, category = false }) {
  const secondaryField = category ? 'descripcion' : 'biografia';
  return (
    <form
      className="space-y-3 rounded-3xl border border-white/70 bg-white/85 p-5 shadow-soft backdrop-blur-sm"
      onSubmit={(event) => {
        event.preventDefault();
        onSave();
      }}
    >
      <h2 className="font-display text-lg font-bold text-ink-900">{title}</h2>
      <input required className="input-pastel w-full" aria-label="Nombre" placeholder="Nombre" value={values.nombre} onChange={(event) => setValues({ ...values, nombre: event.target.value })} />
      {!category && (
        <input className="input-pastel w-full" aria-label="Nacionalidad" placeholder="Nacionalidad" value={values.nacionalidad ?? ''} onChange={(event) => setValues({ ...values, nacionalidad: event.target.value })} />
      )}
      <textarea
        className="input-pastel w-full"
        rows={3}
        aria-label={category ? 'Descripción' : 'Biografía'}
        placeholder={category ? 'Descripción' : 'Biografía'}
        value={values[secondaryField] ?? ''}
        onChange={(event) => setValues({ ...values, [secondaryField]: event.target.value })}
      />
      <div className="flex flex-wrap gap-2">
        <button disabled={saving} className={primaryBtn}>
          Guardar
        </button>
        <button type="button" className={ghostBtn} onClick={onCancel}>
          Limpiar
        </button>
      </div>
    </form>
  );
}

function List({ title, items, onEdit, onDelete, label = 'titulo' }) {
  return (
    <div className="rounded-3xl border border-white/70 bg-white/85 p-5 shadow-soft backdrop-blur-sm">
      <h2 className="font-display text-lg font-bold text-ink-900">{title}</h2>
      <div className="mt-3 space-y-2">
        {items.length === 0 ? (
          <p className="text-sm text-ink-500">Sin datos.</p>
        ) : (
          items.map((item) => (
            <div key={item.id} className="flex items-center justify-between gap-3 rounded-2xl bg-cream-50 p-3">
              <span className="text-sm font-semibold text-ink-900">{item[label]}</span>
              <div className="flex gap-2">
                <button className={editBtn} onClick={() => onEdit(item)}>
                  Editar
                </button>
                <button className={dangerBtn} onClick={() => onDelete(item)}>
                  Eliminar
                </button>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
