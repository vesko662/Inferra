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
            Live prices, ML-powered forecasts, and news sentiment — all in one place.
            Inferra gives you a sharper view of the market without the noise.
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
          <span className="stat-card__label">Price data</span>
          <strong>Real-time snapshots</strong>
          <p>Live prices, 24h change, volume, and market cap refreshed every few minutes from Binance.</p>
        </article>
        <article className="surface-card stat-card">
          <span className="stat-card__label">ML forecasts</span>
          <strong>3 prediction models</strong>
          <p>Linear Regression, LSTM, and XGBoost generate multi-day price forecasts from historical data.</p>
        </article>
        <article className="surface-card stat-card">
          <span className="stat-card__label">Sentiment</span>
          <strong>News-driven signals</strong>
          <p>Headlines are classified as bullish, bearish, or neutral to surface the current market mood.</p>
        </article>
      </section>

      <section id="market-overview" className="container page-section">
        <CoinListSection
          title="Market overview"
          description="Live snapshot data for the tracked assets."
          limit={10}
          showViewAll
        />
      </section>
    </div>
  );
}

export default DashboardPage;
