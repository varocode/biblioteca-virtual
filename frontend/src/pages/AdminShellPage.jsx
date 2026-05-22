export default function AdminShellPage() {
  return (
    <section className="rounded-xl border bg-white p-6 shadow-sm">
      <p className="text-sm font-semibold uppercase tracking-wide text-blue-700">Administración</p>
      <h1 className="text-2xl font-bold">Shell administrativo</h1>
      <p className="mt-2 text-slate-600">Este PR solo habilita la ruta protegida y navegación por rol. CRUD, circulación y dashboard visual quedan para PR 6.</p>
    </section>
  );
}
