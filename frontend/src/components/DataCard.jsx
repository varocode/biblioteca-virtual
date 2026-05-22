const TONES = {
  lavender: {
    bg: 'bg-gradient-to-br from-lavender-100 via-white to-white',
    chip: 'bg-lavender-500/15 text-lavender-700',
    ring: 'ring-lavender-200/60'
  },
  peach: {
    bg: 'bg-gradient-to-br from-peach-100 via-white to-white',
    chip: 'bg-peach-500/15 text-peach-700',
    ring: 'ring-peach-200/60'
  },
  mint: {
    bg: 'bg-gradient-to-br from-mint-100 via-white to-white',
    chip: 'bg-mint-500/15 text-mint-700',
    ring: 'ring-mint-200/60'
  },
  sky: {
    bg: 'bg-gradient-to-br from-sky-100 via-white to-white',
    chip: 'bg-sky-500/15 text-sky-700',
    ring: 'ring-sky-200/60'
  }
};

export default function DataCard({ title, value, children, tone = 'lavender', icon }) {
  const palette = TONES[tone] ?? TONES.lavender;
  return (
    <article className={`relative overflow-hidden rounded-3xl border border-white/70 p-5 shadow-soft ring-1 ${palette.ring} ${palette.bg}`}>
      <div className="flex items-center justify-between gap-3">
        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-ink-500">{title}</p>
        {icon && <span className={`grid h-9 w-9 place-items-center rounded-2xl text-lg ${palette.chip}`}>{icon}</span>}
      </div>
      <p className="mt-3 font-display text-3xl font-extrabold text-ink-900">{value}</p>
      {children && <div className="mt-3 text-sm text-ink-500">{children}</div>}
    </article>
  );
}
