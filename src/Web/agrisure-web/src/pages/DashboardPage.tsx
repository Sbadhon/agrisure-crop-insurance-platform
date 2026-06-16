import { useEffect, useState } from 'react';
import { CircleDollarSign, ClipboardCheck, Files, TimerReset } from 'lucide-react';
import { api } from '../api';
import { MetricCard } from '../components/MetricCard';
import { StatusPill } from '../components/StatusPill';
import type { Actor, Dashboard } from '../types';

const money = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
});

export function DashboardPage({ actor }: { actor: Actor }) {
  const [dashboard, setDashboard] = useState<Dashboard | null>(null);
  const [error, setError] = useState<string>();

  useEffect(() => {
    setError(undefined);
    api
      .getDashboard(actor)
      .then(setDashboard)
      .catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Dashboard failed to load.'));
  }, [actor]);

  if (actor.role === 'Producer' || actor.role === 'Adjuster') {
    return (
      <section className="empty-state">
        <ClipboardCheck size={34} />
        <h2>Operations dashboard is role-restricted</h2>
        <p>Switch to Agent, ClaimsReviewer, or Operations to view the cross-service read model.</p>
      </section>
    );
  }

  if (error) return <ApiError message={error} />;
  if (!dashboard) return <Loading />;

  return (
    <div className="page-stack">
      <section className="hero-panel">
        <div>
          <p className="eyebrow">2026 crop year</p>
          <h2>Claims operations command center</h2>
          <p>
            A read-optimized view updated asynchronously from domain events. Refresh after each workflow action to observe eventual consistency.
          </p>
        </div>
        <div className="hero-badge">Live projection</div>
      </section>

      <section className="metrics-grid">
        <MetricCard label="Total claims" value={dashboard.totalClaims} detail="Portfolio records" icon={<Files size={22} />} />
        <MetricCard label="Open claims" value={dashboard.openClaims} detail="Require workflow completion" icon={<TimerReset size={22} />} />
        <MetricCard label="Awaiting review" value={dashboard.awaitingReview} detail="Inspection completed" icon={<ClipboardCheck size={22} />} />
        <MetricCard label="Est. indemnity" value={money.format(dashboard.portfolioIndemnity)} detail="Demonstration calculation" icon={<CircleDollarSign size={22} />} />
      </section>

      <section className="dashboard-grid">
        <article className="panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">Pipeline</p>
              <h3>Claims by status</h3>
            </div>
          </div>
          <div className="pipeline-list">
            {Object.entries(dashboard.byStatus).map(([status, count]) => (
              <div className="pipeline-row" key={status}>
                <StatusPill status={status} />
                <div className="pipeline-bar"><span style={{ width: `${Math.max(12, count * 24)}%` }} /></div>
                <strong>{count}</strong>
              </div>
            ))}
            {Object.keys(dashboard.byStatus).length === 0 && <p className="muted">Events are still being projected.</p>}
          </div>
        </article>

        <article className="panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">Event stream</p>
              <h3>Recent claim updates</h3>
            </div>
          </div>
          <div className="activity-list">
            {dashboard.recentClaims.map((claim) => (
              <div className="activity-item" key={claim.claimId}>
                <div className="activity-dot" />
                <div>
                  <strong>{claim.claimNumber}</strong>
                  <p>{claim.lastNote}</p>
                  <small>{new Date(claim.updatedAtUtc).toLocaleString()}</small>
                </div>
                <StatusPill status={claim.status} />
              </div>
            ))}
            {dashboard.recentClaims.length === 0 && <p className="muted">Waiting for the claims outbox to publish.</p>}
          </div>
        </article>
      </section>
    </div>
  );
}

function Loading() {
  return <div className="loading">Loading operations projection…</div>;
}

function ApiError({ message }: { message: string }) {
  return (
    <section className="empty-state danger">
      <h2>API is unavailable</h2>
      <p>{message}</p>
      <code>docker compose up --build</code>
    </section>
  );
}
