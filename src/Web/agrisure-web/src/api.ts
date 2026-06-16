import type {
  Actor,
  ClaimDetail,
  ClaimSummary,
  Dashboard,
  PolicyDetail,
  PolicySummary,
} from './types';

const headersFor = (actor: Actor): HeadersInit => ({
  'Content-Type': 'application/json',
  'X-Tenant-Id': actor.tenantId,
  'X-Actor-Id': actor.actorId,
  'X-Actor-Name': actor.actorName,
  'X-Role': actor.role,
});

async function request<T>(
  path: string,
  actor: Actor,
  init?: RequestInit,
): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: {
      ...headersFor(actor),
      ...(init?.headers ?? {}),
    },
  });

  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as
      | { detail?: string; message?: string; title?: string }
      | null;
    throw new Error(
      problem?.detail ??
        problem?.message ??
        problem?.title ??
        `Request failed with status ${response.status}`,
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const api = {
  listPolicies: (actor: Actor) =>
    request<PolicySummary[]>('/api/policies', actor),

  getPolicy: (policyId: string, actor: Actor) =>
    request<PolicyDetail>(`/api/policies/${policyId}`, actor),

  listClaims: (actor: Actor) => request<ClaimSummary[]>('/api/claims', actor),

  getClaim: (claimId: string, actor: Actor) =>
    request<ClaimDetail>(`/api/claims/${claimId}`, actor),

  reportLoss: (
    actor: Actor,
    payload: {
      policyId: string;
      fieldId: string;
      lossDate: string;
      lossCause: string;
      description: string;
      producerActorId?: string;
    },
  ) =>
    request<ClaimDetail>('/api/claims', actor, {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  assignAdjuster: (claimId: string, actor: Actor) =>
    request<ClaimDetail>(`/api/claims/${claimId}/assign`, actor, {
      method: 'POST',
      body: JSON.stringify({
        adjusterId: 'adjuster-3001',
        adjusterName: 'Morgan Lee',
      }),
    }),

  recordInspection: (
    claimId: string,
    actor: Actor,
    actualProduction: number,
    notes: string,
  ) =>
    request<ClaimDetail>(`/api/claims/${claimId}/inspection`, actor, {
      method: 'POST',
      body: JSON.stringify({ actualProduction, notes }),
    }),

  approveClaim: (claimId: string, actor: Actor) =>
    request<ClaimDetail>(`/api/claims/${claimId}/approve`, actor, {
      method: 'POST',
    }),

  requestPayment: (claimId: string, actor: Actor) =>
    request<ClaimDetail>(`/api/claims/${claimId}/request-payment`, actor, {
      method: 'POST',
    }),

  markPaid: (claimId: string, actor: Actor) =>
    request<ClaimDetail>(`/api/claims/${claimId}/mark-paid`, actor, {
      method: 'POST',
    }),

  getDashboard: (actor: Actor) =>
    request<Dashboard>('/api/operations/dashboard', actor),
};
