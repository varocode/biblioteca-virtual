const STYLES = {
  info: 'border-lavender-200 bg-lavender-50/80 text-lavender-700',
  error: 'border-peach-200 bg-peach-100/70 text-peach-700',
  success: 'border-mint-200 bg-mint-100/70 text-mint-700',
  warning: 'border-sun-200 bg-sun-100/80 text-sun-700'
};

const ICONS = {
  info: 'ℹ',
  error: '!',
  success: '✓',
  warning: '!'
};

export default function StatusMessage({ type = 'info', children }) {
  const style = STYLES[type] ?? STYLES.info;
  return (
    <div className={`flex items-start gap-3 rounded-2xl border px-4 py-3 text-sm shadow-sm ${style}`}>
      <span className="mt-0.5 grid h-6 w-6 flex-none place-items-center rounded-full bg-white/70 text-xs font-bold">
        {ICONS[type] ?? ICONS.info}
      </span>
      <div className="leading-relaxed">{children}</div>
    </div>
  );
}
