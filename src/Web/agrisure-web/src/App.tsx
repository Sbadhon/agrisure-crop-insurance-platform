import { useState } from 'react';
import { Layout, type PageKey } from './components/Layout';
import { ClaimDetailPage } from './pages/ClaimDetailPage';
import { ClaimsPage } from './pages/ClaimsPage';
import { DashboardPage } from './pages/DashboardPage';
import { PoliciesPage } from './pages/PoliciesPage';
import type { Actor } from './types';

const actors: Actor[] = [
  { tenantId: 'northstar-agency', actorId: 'agent-2001', actorName: 'Avery Johnson', role: 'Agent' },
  { tenantId: 'northstar-agency', actorId: 'producer-1001', actorName: 'Jordan Miller', role: 'Producer' },
  { tenantId: 'northstar-agency', actorId: 'reviewer-4001', actorName: 'Casey Patel', role: 'ClaimsReviewer' },
  { tenantId: 'northstar-agency', actorId: 'adjuster-3001', actorName: 'Morgan Lee', role: 'Adjuster' },
  { tenantId: 'northstar-agency', actorId: 'ops-5001', actorName: 'Riley Chen', role: 'Operations' },
];

export default function App() {
  const [page, setPage] = useState<PageKey>('dashboard');
  const [actor, setActor] = useState<Actor>(actors[0]!);
  const [claimId, setClaimId] = useState<string>();

  const navigate = (nextPage: PageKey) => {
    setClaimId(undefined);
    setPage(nextPage);
  };

  return (
    <Layout
      page={claimId ? 'claims' : page}
      actor={actor}
      actors={actors}
      onNavigate={navigate}
      onActorChange={setActor}
    >
      {claimId ? (
        <ClaimDetailPage claimId={claimId} actor={actor} onBack={() => setClaimId(undefined)} />
      ) : page === 'dashboard' ? (
        <DashboardPage actor={actor} />
      ) : page === 'policies' ? (
        <PoliciesPage actor={actor} />
      ) : (
        <ClaimsPage actor={actor} onOpenClaim={setClaimId} />
      )}
    </Layout>
  );
}
