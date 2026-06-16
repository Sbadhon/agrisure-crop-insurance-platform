import { FormEvent, useEffect, useState } from 'react';
import { FilePlus2, Search } from 'lucide-react';
import { api } from '../api';
import { StatusPill } from '../components/StatusPill';
import type { Actor, ClaimSummary, PolicyDetail, PolicySummary } from '../types';

export function ClaimsPage({
  actor,
  onOpenClaim,
}: {
  actor: Actor;
  onOpenClaim: (claimId: string) => void;
}) {
  const [claims, setClaims] = useState<ClaimSummary[]>([]);
  const [policies, setPolicies] = useState<PolicySummary[]>([]);
  const [policyDetail, setPolicyDetail] = useState<PolicyDetail>();
  const [showForm, setShowForm] = useState(false);
  const [error, setError] = useState<string>();
  const [saving, setSaving] = useState(false);

  const reload = () => {
    setError(undefined);
    api.listClaims(actor).then(setClaims).catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Claims failed to load.'));
  };

  useEffect(() => {
    reload();
    setShowForm(false);
    if (actor.role === 'Producer' || actor.role === 'Agent') {
      api.listPolicies(actor).then(async (items) => {
        setPolicies(items);
        const first = items[0];
        if (first) setPolicyDetail(await api.getPolicy(first.id, actor));
      }).catch(() => setPolicies([]));
    } else {
      setPolicies([]);
      setPolicyDetail(undefined);
    }
  }, [actor]);

  const selectPolicy = async (policyId: string) => {
    setPolicyDetail(await api.getPolicy(policyId, actor));
  };

  const submitLoss = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!policyDetail) return;
    const form = new FormData(event.currentTarget);
    setSaving(true);
    setError(undefined);
    try {
      const claim = await api.reportLoss(actor, {
        policyId: policyDetail.id,
        fieldId: String(form.get('fieldId')),
        lossDate: String(form.get('lossDate')),
        lossCause: String(form.get('lossCause')),
        description: String(form.get('description')),
        producerActorId: 'producer-1001',
      });
      setShowForm(false);
      reload();
      onOpenClaim(claim.id);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Notice of Loss could not be submitted.');
    } finally {
      setSaving(false);
    }
  };

  const canReport = actor.role === 'Producer' || actor.role === 'Agent';

  return (
    <div className="page-stack">
      <section className="toolbar">
        <div className="search-box"><Search size={18} /><span>{claims.length} claim records</span></div>
        {canReport && (
          <button className="button primary" onClick={() => setShowForm((value) => !value)}>
            <FilePlus2 size={18} /> Report a loss
          </button>
        )}
      </section>

      {error && <div className="inline-error">{error}</div>}

      {showForm && policyDetail && (
        <form className="panel loss-form" onSubmit={submitLoss}>
          <div className="panel-heading">
            <div><p className="eyebrow">Producer self-service</p><h3>Submit Notice of Loss</h3></div>
            <button className="button ghost" type="button" onClick={() => setShowForm(false)}>Cancel</button>
          </div>
          <div className="form-grid">
            <label>Policy<select value={policyDetail.id} onChange={(event) => void selectPolicy(event.target.value)}>{policies.map((policy) => <option value={policy.id} key={policy.id}>{policy.policyNumber}</option>)}</select></label>
            <label>Insured field<select name="fieldId" required>{policyDetail.fields.map((field) => <option value={field.id} key={field.id}>{field.fieldNumber} — {field.insuredAcres.toFixed(2)} acres</option>)}</select></label>
            <label>Loss date<input name="lossDate" type="date" defaultValue="2026-06-12" required /></label>
            <label>Cause of loss<select name="lossCause" defaultValue="Hail"><option>Hail</option><option>Wind</option><option>Drought</option><option>Excess moisture</option><option>Freeze</option></select></label>
            <label className="full-width">Description<textarea name="description" defaultValue="Severe weather damaged the insured crop. Field inspection is requested." required /></label>
          </div>
          <div className="form-actions"><span className="muted">Synthetic demonstration data only.</span><button className="button primary" disabled={saving}>{saving ? 'Submitting…' : 'Submit Notice of Loss'}</button></div>
        </form>
      )}

      <section className="panel">
        <div className="claims-table table-header">
          <span>Claim</span><span>Producer</span><span>Loss</span><span>Status</span><span>Updated</span>
        </div>
        {claims.map((claim) => (
          <button className="claims-table table-row" key={claim.id} onClick={() => onOpenClaim(claim.id)}>
            <span><strong>{claim.claimNumber}</strong><small>{claim.policyNumber} · {claim.fieldNumber}</small></span>
            <span><strong>{claim.producerName}</strong><small>{claim.crop} · {claim.county}</small></span>
            <span><strong>{claim.lossCause}</strong><small>{claim.lossDate}</small></span>
            <span><StatusPill status={claim.status} /></span>
            <span><strong>{new Date(claim.updatedAtUtc).toLocaleDateString()}</strong><small>{claim.assignedAdjusterName ?? 'Unassigned'}</small></span>
          </button>
        ))}
        {claims.length === 0 && <div className="empty-row">No claims are visible for this actor.</div>}
      </section>
    </div>
  );
}
