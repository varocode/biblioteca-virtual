export default function PageHeader({ eyebrow, title, children, action }) {
  return (
    <div className="flex flex-wrap items-end justify-between gap-4">
      <div className="max-w-2xl">
        {eyebrow && (
          <p className="inline-flex items-center gap-2 rounded-full bg-lavender-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] text-lavender-700">
            <span className="h-1.5 w-1.5 rounded-full bg-lavender-500" />
            {eyebrow}
          </p>
        )}
        <h1 className="mt-3 font-display text-4xl font-extrabold tracking-tight text-ink-900">{title}</h1>
        {children && <p className="mt-3 text-base text-ink-500">{children}</p>}
      </div>
      {action}
    </div>
  );
}
