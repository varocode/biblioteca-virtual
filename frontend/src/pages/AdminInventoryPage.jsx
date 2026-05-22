import { useEffect, useState } from 'react';
import AdminTabs from '../components/AdminTabs.jsx';
import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { getErrorMessage } from '../services/api.js';
import { fetchCopies } from '../services/adminService.js';

const STATE_TONES = {
  Disponible: 'bg-mint-100 text-mint-700',
  Prestado: 'bg-peach-100 text-peach-700',
  Reservado: 'bg-sky-100 text-sky-700',
  Mantenimiento: 'bg-sun-100 text-sun-700'
};

export default function AdminInventoryPage() {
  const [state, setState] = useState({ copies: [], loading: true, error: '' });

  useEffect(() => {
    fetchCopies()
      .then((copies) => setState({ copies, loading: false, error: '' }))
      .catch((err) =>
        setState({ copies: [], loading: false, error: getErrorMessage(err, 'No se pudieron cargar los ejemplares.') })
      );
  }, []);

  return (
    <section className="space-y-6">
      <AdminTabs />
      <PageHeader eyebrow="Administración" title="Inventario de ejemplares">
        Consulta operativa de códigos, estados y ubicación de copias.
      </PageHeader>

      {state.error && <StatusMessage type="error">{state.error}</StatusMessage>}
      {state.loading ? (
        <StatusMessage>Cargando ejemplares...</StatusMessage>
      ) : (
        <div className="overflow-hidden rounded-3xl border border-white/70 bg-white/85 shadow-soft backdrop-blur-sm">
          <table className="min-w-full text-left text-sm">
            <thead className="bg-lavender-50/70 text-ink-700">
              <tr>
                <th className="p-4 font-semibold">Libro</th>
                <th className="p-4 font-semibold">Código</th>
                <th className="p-4 font-semibold">Estado</th>
                <th className="p-4 font-semibold">Tipo</th>
                <th className="p-4 font-semibold">Ubicación</th>
              </tr>
            </thead>
            <tbody>
              {state.copies.map((copy) => {
                const tone = STATE_TONES[copy.detalle.estado] || 'bg-lavender-100 text-lavender-700';
                return (
                  <tr key={copy.id} className="border-t border-lavender-100/60">
                    <td className="p-4 font-semibold text-ink-900">{copy.libroTitulo}</td>
                    <td className="p-4 text-ink-700">{copy.detalle.codigo}</td>
                    <td className="p-4">
                      <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-[11px] font-semibold ${tone}`}>
                        {copy.detalle.estado}
                      </span>
                    </td>
                    <td className="p-4 text-ink-700">{copy.detalle.tipo}</td>
                    <td className="p-4 text-ink-700">{copy.detalle.ubicacion ?? '—'}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
