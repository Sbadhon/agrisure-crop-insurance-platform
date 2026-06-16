import { useEffect, useState, type ReactNode } from 'react';
import { CalendarDays, MapPinned, Sprout } from 'lucide-react';
import { api } from '../api';
import { FieldMap } from '../components/FieldMap';
import { StatusPill } from '../components/StatusPill';
import type { Actor, PolicyDetail, PolicySummary } from '../types';

export function PoliciesPage({ actor }: { actor: Actor }) {
  const [policies, setPolicies] = useState<PolicySummary[]>([]);
  const [selected, setSelected] = useState<PolicyDetail>();
  const [error, setError] = useState<string>();

  useEffect(() => {
    setError(undefined);
    setSelected(undefined);
    api
      .listPolicies(actor)
      .then(async (items) => {
        setPolicies(items);
        const first = items[0];
        if (first) setSelected(await api.getPolicy(first.id, actor));
      })
      .catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Policies failed to load.'));
  }, [actor]);

  const choosePolicy = async (policyId: string) => {
    try {
      setSelected(await api.getPolicy(policyId, actor));
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Policy failed to load.');
    }
  };

  if (error) return <section className="empty-state danger"><h2>Unable to load policies</h2><p>{error}</p></section>;

  return (
    <div className="split-layout">
      <section className="panel list-panel">
        <div className="panel-heading">
          <div><p className="eyebrow">Book of business</p><h3>Bound policies</h3></div>
          <span className="count-badge">{policies.length}</span>
        </div>
        <div className="record-list">
          {policies.map((policy) => (
            <button
              className={selected?.id === policy.id ? 'record-card selected' : 'record-card'}
              key={policy.id}
              onClick={() => void choosePolicy(policy.id)}
            >
              <div><strong>{policy.policyNumber}</strong><span>{policy.producerName}</span></div>
              <StatusPill status={policy.status} />
              <div className="record-meta"><span>{policy.crop} · {policy.cropYear}</span><span>{policy.totalInsuredAcres.toFixed(2)} ac</span></div>
            </button>
          ))}
          {policies.length === 0 && <p className="muted">No policies are visible for this actor.</p>}
        </div>
      </section>

      {selected ? (
        <section className="detail-stack">
          <article className="panel policy-overview">
            <div className="panel-heading">
              <div><p className="eyebrow">{selected.policyNumber}</p><h2>{selected.producerName}</h2></div>
              <StatusPill status={selected.status} />
            </div>
            <div className="detail-facts">
              <Fact icon={<Sprout size={18} />} label="Crop" value={`${selected.crop} · ${selected.cropYear}`} />
              <Fact icon={<MapPinned size={18} />} label="County" value={`${selected.county}, ${selected.state}`} />
              <Fact icon={<CalendarDays size={18} />} label="Policy term" value={`${selected.effectiveDate} — ${selected.expirationDate}`} />
            </div>
            <div className="coverage-strip">
              <div><span>Coverage level</span><strong>{Math.round(selected.coverageLevel * 100)}%</strong></div>
              <div><span>Approved yield</span><strong>{selected.approvedYield} bu/ac</strong></div>
              <div><span>Demo price</span><strong>${selected.demonstrationPrice.toFixed(2)}</strong></div>
            </div>
          </article>

          <article className="panel">
            <div className="panel-heading"><div><p className="eyebrow">Digital acreage</p><h3>Insured field boundaries</h3></div></div>
            <FieldMap fields={selected.fields} />
            <div className="field-table">
              {selected.fields.map((field) => (
                <div className="field-row" key={field.id}>
                  <strong>{field.fieldNumber}</strong>
                  <span>Farm {field.farmNumber}</span>
                  <span>Tract {field.tractNumber}</span>
                  <span>{field.plantingDate}</span>
                  <strong>{field.insuredAcres.toFixed(2)} ac</strong>
                </div>
              ))}
            </div>
          </article>
        </section>
      ) : (
        <section className="empty-state"><MapPinned size={34} /><h2>Select a policy</h2></section>
      )}
    </div>
  );
}

function Fact({ icon, label, value }: { icon: ReactNode; label: string; value: string }) {
  return <div className="fact"><span className="fact-icon">{icon}</span><div><small>{label}</small><strong>{value}</strong></div></div>;
}
