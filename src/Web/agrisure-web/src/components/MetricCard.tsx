import type { ReactNode } from 'react';

export function MetricCard({
  label,
  value,
  detail,
  icon,
}: {
  label: string;
  value: string | number;
  detail: string;
  icon: ReactNode;
}) {
  return (
    <article className="metric-card">
      <div className="metric-icon">{icon}</div>
      <div>
        <p className="eyebrow">{label}</p>
        <strong className="metric-value">{value}</strong>
        <p className="muted metric-detail">{detail}</p>
      </div>
    </article>
  );
}
