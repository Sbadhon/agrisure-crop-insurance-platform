export type Role =
  | 'Producer'
  | 'Agent'
  | 'Adjuster'
  | 'ClaimsReviewer'
  | 'Operations';

export type Actor = {
  tenantId: string;
  actorId: string;
  actorName: string;
  role: Role;
};

export type PolicySummary = {
  id: string;
  policyNumber: string;
  producerId: string;
  producerName: string;
  crop: string;
  cropYear: number;
  county: string;
  state: string;
  status: string;
  coverageLevel: number;
  totalInsuredAcres: number;
};

export type InsuredField = {
  id: string;
  fieldNumber: string;
  farmNumber: string;
  tractNumber: string;
  insuredAcres: number;
  plantingDate: string;
  geoJson: string;
};

export type PolicyDetail = PolicySummary & {
  approvedYield: number;
  demonstrationPrice: number;
  effectiveDate: string;
  expirationDate: string;
  fields: InsuredField[];
};

export type ClaimSummary = {
  id: string;
  claimNumber: string;
  policyNumber: string;
  producerName: string;
  crop: string;
  county: string;
  fieldNumber: string;
  lossDate: string;
  lossCause: string;
  status: string;
  assignedAdjusterName?: string | null;
  estimatedIndemnity?: number | null;
  updatedAtUtc: string;
};

export type TimelineEntry = {
  id: string;
  status: string;
  note: string;
  actorName: string;
  actorRole: string;
  occurredAtUtc: string;
};

export type ClaimDetail = {
  id: string;
  claimNumber: string;
  policyId: string;
  policyNumber: string;
  producerId: string;
  producerName: string;
  crop: string;
  county: string;
  fieldId: string;
  fieldNumber: string;
  insuredAcres: number;
  lossDate: string;
  lossCause: string;
  description: string;
  status: string;
  assignedAdjusterId?: string | null;
  assignedAdjusterName?: string | null;
  actualProduction?: number | null;
  inspectionNotes?: string | null;
  approvedYield: number;
  coverageLevel: number;
  demonstrationPrice: number;
  estimatedIndemnity?: number | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  timeline: TimelineEntry[];
};

export type OperationsClaim = {
  claimId: string;
  claimNumber: string;
  policyNumber: string;
  producerName: string;
  crop: string;
  county: string;
  status: string;
  estimatedIndemnity?: number | null;
  lastNote: string;
  updatedAtUtc: string;
};

export type Dashboard = {
  totalClaims: number;
  openClaims: number;
  awaitingReview: number;
  portfolioIndemnity: number;
  byStatus: Record<string, number>;
  recentClaims: OperationsClaim[];
};
