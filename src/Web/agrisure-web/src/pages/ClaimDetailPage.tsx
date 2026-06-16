import { FormEvent, useEffect, useState } from 'react';
import { ArrowLeft, CircleDollarSign, ClipboardCheck, UserRoundCheck } from 'lucide-react';
import { api } from '../api';
import { StatusPill } from '../components/StatusPill';
import type { Actor, ClaimDetail } from '../types';

const money = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });

export function ClaimDetailPage({
  claimId,
  actor,
  onBack,
}: {
  claimId: string;
  actor: Actor;
  onBack: () => void;
}) {
  const [claim, setClaim] = useState<ClaimDetail>();
  const [error, setError] = useState<string>();
  const [saving, setSaving] = useState(false);

  const load = () => api.getClaim(claimId, actor).then(setClaim).catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Claim failed to load.'));

  useEffect(() => {
    setClaim(undefined);
    setError(undefined);
    load();
  }, [claimId, actor]);

  const run = async (action: () => Promise<ClaimDetail>) => {
    setSaving(true);
    setError(undefined);
    try {
      setClaim(await action());
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Action failed.');
    } finally {
      setSaving(false);
    }
  };

  const submitInspection = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    void run(() => api.recordInspection(claimId, actor, Number(form.get('actualProduction')), String(form.get('notes'))));
  };

  if (error && !claim) return <section className="empty-state danger"><h2>Unable to open claim</h2><p>{error}</p><button className="button" onClick={onBack}>Back to claims</button></section>;
  if (!claim) return <div className="loading">Loading claim workflow…</div>;

  return (
    <div className="page-stack">
      <button className="back-link" onClick={onBack}><ArrowLeft size={17} /> Back to claims</button>
      {error && <div className="inline-error">{error}</div>}

      <section className="panel claim-header">
        <div>
          <p className="eyebrow">{claim.policyNumber}</p>
          <h2>{claim.claimNumber}</h2>
          <p>{claim.producerName} · {claim.crop} · Field {claim.fieldNumber}</p>
        </div>
        <StatusPill status={claim.status} />
      </section>

      <section className="claim-detail-grid">
        <article className="panel">
          <div className="panel-heading"><div><p className="eyebrow">Workflow history</p><h3>Claim timeline</h3></div></div>
          <div className="timeline">
            {claim.timeline.map((entry) => (
              <div className="timeline-entry" key={entry.id}>
                <div className="timeline-marker" />
                <div>
                  <div className="timeline-title"><StatusPill status={entry.status} /><time>{new Date(entry.occurredAtUtc).toLocaleString()}</time></div>
                  <p>{entry.note}</p>
                  <small>{entry.actorName} · {entry.actorRole}</small>
                </div>
              </div>
            ))}
          </div>
        </article>

        <aside className="detail-stack">
          <article className="panel action-panel">
            <div className="panel-heading"><div><p className="eyebrow">Next action</p><h3>Role-aware workflow</h3></div></div>
            <WorkflowAction claim={claim} actor={actor} saving={saving} run={run} submitInspection={submitInspection} />
          </article>

          <article className="panel calculation-card">
            <div className="panel-heading"><div><p className="eyebrow">Demonstration only</p><h3>Indemnity calculation</h3></div><CircleDollarSign size={22} /></div>
            <CalculationRow label="Approved yield" value={`${claim.approvedYield} bu/ac`} />
            <CalculationRow label="Coverage level" value={`${Math.round(claim.coverageLevel * 100)}%`} />
            <CalculationRow label="Insured acres" value={claim.insuredAcres.toFixed(2)} />
            <CalculationRow label="Actual production" value={claim.actualProduction?.toLocaleString() ?? 'Pending inspection'} />
            <CalculationRow label="Demo price" value={`$${claim.demonstrationPrice.toFixed(2)}`} />
            <div className="calculation-total"><span>Estimated indemnity</span><strong>{claim.estimatedIndemnity == null ? 'Pending approval' : money.format(claim.estimatedIndemnity)}</strong></div>
          </article>
        </aside>
      </section>
    </div>
  );
}

function WorkflowAction({
  claim,
  actor,
  saving,
  run,
  submitInspection,
}: {
  claim: ClaimDetail;
  actor: Actor;
  saving: boolean;
  run: (action: () => Promise<ClaimDetail>) => Promise<void>;
  submitInspection: (event: FormEvent<HTMLFormElement>) => void;
}) {
  if (claim.status === 'LossReported' && (actor.role === 'ClaimsReviewer' || actor.role === 'Operations')) {
    return <button className="button primary full" disabled={saving} onClick={() => void run(() => api.assignAdjuster(claim.id, actor))}><UserRoundCheck size={18} /> Assign Morgan Lee</button>;
  }

  if (claim.status === 'AdjusterAssigned' && actor.role === 'Adjuster') {
    return (
      <form className="compact-form" onSubmit={submitInspection}>
        <label>Actual production (bushels)<input name="actualProduction" type="number" min="0" step="0.01" defaultValue="12000" required /></label>
        <label>Inspection notes<textarea name="notes" defaultValue="Hail impact confirmed across the northern field. Production evidence reviewed." required /></label>
        <button className="button primary full" disabled={saving}><ClipboardCheck size={18} /> Complete inspection</button>
      </form>
    );
  }

  if (claim.status === 'InspectionCompleted' && actor.role === 'ClaimsReviewer') {
    return <button className="button primary full" disabled={saving} onClick={() => void run(() => api.approveClaim(claim.id, actor))}>Approve claim</button>;
  }

  if (claim.status === 'Approved' && actor.role === 'ClaimsReviewer') {
    return <button className="button primary full" disabled={saving} onClick={() => void run(() => api.requestPayment(claim.id, actor))}>Request payment</button>;
  }

  if (claim.status === 'PaymentRequested' && actor.role === 'Operations') {
    return <button className="button primary full" disabled={saving} onClick={() => void run(() => api.markPaid(claim.id, actor))}>Confirm simulated payment</button>;
  }

  if (claim.status === 'Paid') {
    return <div className="success-message"><ClipboardCheck size={22} /><strong>Workflow complete</strong><span>The settlement simulator confirmed payment.</span></div>;
  }

  return (
    <div className="role-hint">
      <strong>No action for {actor.role}</strong>
      <span>{nextRoleHint(claim.status)}</span>
    </div>
  );
}

function nextRoleHint(status: string): string {
  const hints: Record<string, string> = {
    LossReported: 'Switch to ClaimsReviewer or Operations to assign an adjuster.',
    AdjusterAssigned: 'Switch to Adjuster to complete the field inspection.',
    InspectionCompleted: 'Switch to ClaimsReviewer to approve the claim.',
    Approved: 'Switch to ClaimsReviewer to request payment.',
    PaymentRequested: 'Switch to Operations to confirm payment.',
  };
  return hints[status] ?? 'This workflow state has no configured action.';
}

function CalculationRow({ label, value }: { label: string; value: string }) {
  return <div className="calculation-row"><span>{label}</span><strong>{value}</strong></div>;
}
