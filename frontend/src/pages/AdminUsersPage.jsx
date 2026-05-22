import { useEffect, useState } from 'react';
import AdminTabs from '../components/AdminTabs.jsx';
import PageHeader from '../components/PageHeader.jsx';
import StatusMessage from '../components/StatusMessage.jsx';
import { getErrorMessage } from '../services/api.js';
import { fetchUsers, updateUser } from '../services/adminService.js';
import { enumLabel, roleLabel } from '../utils/formatters.js';

export default function AdminUsersPage() {
  const [users, setUsers] = useState([]);
  const [status, setStatus] = useState({ loading: true, saving: false, error: '', success: '' });

  const load = () =>
    fetchUsers()
      .then(setUsers)
      .catch((err) =>
        setStatus((s) => ({ ...s, error: getErrorMessage(err, 'No se pudieron cargar usuarios.') }))
      )
      .finally(() => setStatus((s) => ({ ...s, loading: false })));
  useEffect(() => {
    load();
  }, []);

  async function toggle(user) {
    if (!confirm(`${user.activo ? 'Desactivar' : 'Activar'} usuario ${user.nombre}?`)) return;
    setStatus((s) => ({ ...s, saving: true, error: '', success: '' }));
    try {
      await updateUser(user.id, {
        nombre: user.nombre,
        rol: user.rol,
        activo: !user.activo,
        telefono: user.telefono,
        direccion: user.direccion
      });
      await load();
      setStatus({ loading: false, saving: false, error: '', success: 'Usuario actualizado.' });
    } catch (err) {
      setStatus({ loading: false, saving: false, error: getErrorMessage(err), success: '' });
    }
  }

  return (
    <section className="space-y-6">
      <AdminTabs />
      <PageHeader eyebrow="Administración" title="Usuarios">
        Gestiona estado y roles básicos de lectores y administradores.
      </PageHeader>
      {status.error && <StatusMessage type="error">{status.error}</StatusMessage>}
      {status.success && <StatusMessage type="success">{status.success}</StatusMessage>}

      {status.loading ? (
        <StatusMessage>Cargando usuarios...</StatusMessage>
      ) : (
        <div className="overflow-hidden rounded-3xl border border-white/70 bg-white/85 shadow-soft backdrop-blur-sm">
          <table className="min-w-full text-left text-sm">
            <thead className="bg-lavender-50/70 text-ink-700">
              <tr>
                <th className="p-4 font-semibold">Nombre</th>
                <th className="p-4 font-semibold">Email</th>
                <th className="p-4 font-semibold">Rol</th>
                <th className="p-4 font-semibold">Estado</th>
                <th className="p-4 font-semibold">Acción</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id} className="border-t border-lavender-100/60">
                  <td className="p-4 font-semibold text-ink-900">{user.nombre}</td>
                  <td className="p-4 text-ink-700">{user.email}</td>
                  <td className="p-4">
                    <span className="inline-flex items-center rounded-full bg-lavender-100 px-2.5 py-0.5 text-[11px] font-semibold text-lavender-700">
                      {enumLabel(user.rol, roleLabel)}
                    </span>
                  </td>
                  <td className="p-4">
                    <span
                      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-[11px] font-semibold ${
                        user.activo ? 'bg-mint-100 text-mint-700' : 'bg-peach-100 text-peach-700'
                      }`}
                    >
                      {user.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td className="p-4">
                    <button
                      className={`rounded-full px-4 py-2 text-xs font-semibold transition disabled:opacity-50 ${
                        user.activo
                          ? 'border border-lavender-200 bg-white text-ink-900 hover:border-peach-300 hover:bg-peach-100 hover:text-peach-700'
                          : 'bg-gradient-to-r from-mint-500 to-mint-700 text-white shadow-glow'
                      }`}
                      disabled={status.saving}
                      onClick={() => toggle(user)}
                    >
                      {user.activo ? 'Desactivar' : 'Activar'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
