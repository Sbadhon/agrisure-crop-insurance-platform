import type { ReactNode } from 'react';
import {
  BarChart3,
  FileWarning,
  LayoutDashboard,
  Leaf,
  MapPinned,
  ShieldCheck,
} from 'lucide-react';
import type { Actor } from '../types';

export type PageKey = 'dashboard' | 'policies' | 'claims';

const navItems: Array<{
  key: PageKey;
  label: string;
  icon: typeof LayoutDashboard;
}> = [
  { key: 'dashboard', label: 'Operations', icon: LayoutDashboard },
  { key: 'policies', label: 'Policies & fields', icon: MapPinned },
  { key: 'claims', label: 'Claims', icon: FileWarning },
];

export function Layout({
  page,
  actor,
  actors,
  onNavigate,
  onActorChange,
  children,
}: {
  page: PageKey;
  actor: Actor;
  actors: Actor[];
  onNavigate: (page: PageKey) => void;
  onActorChange: (actor: Actor) => void;
  children: ReactNode;
}) {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <button className="brand" onClick={() => onNavigate('dashboard')}>
          <span className="brand-mark"><Leaf size={22} /></span>
          <span>
            <strong>AgriSure</strong>
            <small>Crop Insurance Operations</small>
          </span>
        </button>

        <nav className="primary-nav" aria-label="Primary navigation">
          {navItems.map(({ key, label, icon: Icon }) => (
            <button
              className={page === key ? 'nav-item active' : 'nav-item'}
              key={key}
              onClick={() => onNavigate(key)}
            >
              <Icon size={19} />
              {label}
            </button>
          ))}
        </nav>

        <div className="architecture-card">
          <BarChart3 size={20} />
          <strong>Event-driven demo</strong>
          <p>Claims events update an independent operations read model through RabbitMQ.</p>
        </div>

        <div className="sidebar-footer">
          <ShieldCheck size={18} />
          Synthetic data only
        </div>
      </aside>

      <section className="content-shell">
        <header className="topbar">
          <div>
            <p className="eyebrow">NorthStar Crop Agency</p>
            <h1>{navItems.find((item) => item.key === page)?.label}</h1>
          </div>
          <label className="role-switcher">
            <span>Acting as</span>
            <select
              value={actor.role}
              onChange={(event) => {
                const selected = actors.find((item) => item.role === event.target.value);
                if (selected) onActorChange(selected);
              }}
            >
              {actors.map((item) => (
                <option value={item.role} key={item.role}>
                  {item.role} — {item.actorName}
                </option>
              ))}
            </select>
          </label>
        </header>
        <main>{children}</main>
      </section>
    </div>
  );
}
