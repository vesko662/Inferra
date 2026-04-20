import { Link } from "react-router-dom";
import CoinListSection from "../sections/CoinListSection";

function DashboardPage() {
  return (
    <div className="page page--dashboard">
      <section className="hero container">
        <div className="hero__content surface-card">
          <div className="eyebrow">Real-time market visibility</div>
          <h1>Track the crypto market with a clean, focused dashboard.</h1>
          <p>
            Inferra now has a scalable frontend foundation wired around the confirmed Swagger
            endpoints for assets, per-asset details, and latest snapshots.
          </p>

          <div className="hero__actions">
            <Link className="button button--primary" to="/coins">
              Explore markets
            </Link>
            <a className="button button--ghost" href="#market-overview">
              View overview
            </a>
          </div>
        </div>
      </section>

      <section className="container stats-grid" aria-label="Platform highlights">
        <article className="surface-card stat-card">
          <span className="stat-card__label">Refresh cadence</span>
          <strong>Every 5 minutes</strong>
          <p>Price data is designed to auto-refresh on an interval for consistent monitoring.</p>
        </article>
        <article className="surface-card stat-card">
          <span className="stat-card__label">API layer</span>
          <strong>Swagger-aligned</strong>
          <p>Only confirmed endpoints are used, with response rendering kept generic and safe.</p>
        </article>
        <article className="surface-card stat-card">
          <span className="stat-card__label">Future-ready detail views</span>
          <strong>Extensible pages</strong>
          <p>Detail screens are structured to support candles, watchlists, and richer KPI panels.</p>
        </article>
      </section>

      <section id="market-overview" className="container page-section">
        <CoinListSection
          title="Market overview"
          description="A fast-scanning market board with fresh snapshot data for the tracked assets."
          limit={10}
          showViewAll
        />
      </section>
    </div>
  );
}

export default DashboardPage;
