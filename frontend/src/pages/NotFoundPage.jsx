import { Link } from 'react-router-dom';

export default function NotFoundPage() {
  return (
    <section className="mx-auto max-w-xl rounded-3xl border border-white/70 bg-white/85 p-10 text-center shadow-soft backdrop-blur-sm">
      <p className="font-display text-6xl font-extrabold tracking-tight text-lavender-500">404</p>
      <h1 className="mt-2 font-display text-2xl font-bold text-ink-900">Página no encontrada</h1>
      <p className="mt-2 text-sm text-ink-500">Probablemente el enlace cambió o el contenido se movió.</p>
      <Link
        className="mt-5 inline-flex rounded-full bg-gradient-to-r from-lavender-500 to-peach-500 px-5 py-2 text-sm font-semibold text-white shadow-glow transition hover:opacity-95"
        to="/catalogo"
      >
        Volver al catálogo
      </Link>
    </section>
  );
}
