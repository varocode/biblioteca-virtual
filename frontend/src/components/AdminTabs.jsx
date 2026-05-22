import { NavLink } from 'react-router-dom';

const tabClass = ({ isActive }) =>
  `rounded-full px-4 py-2 text-sm font-semibold transition ${
    isActive
      ? 'bg-gradient-to-r from-lavender-500 to-lavender-600 text-white shadow-glow'
      : 'border border-lavender-100 bg-white/85 text-ink-700 hover:border-lavender-300 hover:bg-lavender-50 hover:text-lavender-700'
  }`;

export default function AdminTabs() {
  return (
    <nav className="flex flex-wrap gap-2 rounded-3xl border border-white/70 bg-white/60 p-2 shadow-soft backdrop-blur-sm">
      <NavLink end to="/admin" className={tabClass}>Dashboard</NavLink>
      <NavLink to="/admin/usuarios" className={tabClass}>Usuarios</NavLink>
      <NavLink to="/admin/catalogo" className={tabClass}>Catálogo</NavLink>
      <NavLink to="/admin/circulacion" className={tabClass}>Circulación</NavLink>
      <NavLink to="/admin/inventario" className={tabClass}>Inventario</NavLink>
      <NavLink to="/admin/auditoria" className={tabClass}>Auditoría</NavLink>
    </nav>
  );
}
