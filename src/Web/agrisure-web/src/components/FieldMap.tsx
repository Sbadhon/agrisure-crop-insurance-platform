import type { InsuredField } from '../types';

function pointsFromGeoJson(geoJson: string): string {
  try {
    const geometry = JSON.parse(geoJson) as {
      coordinates?: number[][][];
    };
    return (geometry.coordinates?.[0] ?? [])
      .map(([x, y]) => `${x ?? 0},${y ?? 0}`)
      .join(' ');
  } catch {
    return '12,18 77,12 88,48 61,82 20,73';
  }
}

export function FieldMap({ fields }: { fields: InsuredField[] }) {
  return (
    <div className="field-map" aria-label="Synthetic field boundary map">
      <div className="map-grid" />
      {fields.map((field, index) => (
        <svg
          className={`field-shape field-shape-${index + 1}`}
          viewBox="0 0 100 100"
          role="img"
          aria-label={`${field.fieldNumber}, ${field.insuredAcres} acres`}
          key={field.id}
        >
          <polygon points={pointsFromGeoJson(field.geoJson)} />
          <text x="48" y="49" textAnchor="middle">
            {field.fieldNumber}
          </text>
        </svg>
      ))}
      <div className="map-legend">
        <span>Washington County, MN</span>
        <strong>{fields.reduce((total, field) => total + field.insuredAcres, 0).toFixed(2)} ac</strong>
      </div>
    </div>
  );
}
