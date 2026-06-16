export function StatusPill({ status }: { status: string }) {
  const normalized = status.replace(/([a-z])([A-Z])/g, '$1 $2');
  return <span className={`status status-${status.toLowerCase()}`}>{normalized}</span>;
}
