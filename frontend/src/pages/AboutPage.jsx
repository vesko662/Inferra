import { Link } from "react-router-dom";

const features = [
  {
    title: "Real-time market data",
    description:
      "Live price snapshots, 24h change, volume, and market cap pulled directly from Binance — refreshed every few minutes."
  },
  {
    title: "ML price predictions",
    description:
      "Three machine learning models — Linear Regression, LSTM, and XGBoost — generate multi-day price forecasts trained on historical candle data."
  },
  {
    title: "News sentiment analysis",
    description:
      "Recent news headlines are collected and classified as bullish, bearish, or neutral using a dedicated NLP model, giving you a market mood signal."
  },
  {
    title: "Historical candle charts",
    description:
      "Browse full OHLC history with switchable simple line and candlestick modes, and overlay forecast models directly on the chart."
  }
];

const techStack = [
  { name: "Binance", role: "Market data & candles" },
  { name: "CoinGecko", role: "Asset metadata & logos" },
  { name: "News API", role: "News articles feed" },
  { name: "ASP.NET Core", role: "Backend API" },
  { name: "React", role: "Frontend" },
  { name: "Keycloak", role: "Authentication" }
];

function AboutPage() {
  return (
    <div className="page">
      <section className="hero container">
        <div className="hero__content surface-card">
          <div className="eyebrow">About Inferra</div>
          <h1>Crypto intelligence, simplified.</h1>
          <p>
            Inferra is a full-stack crypto analytics platform combining real-time market data,
            machine learning price forecasts, and news-driven sentiment analysis into one clean
            interface.
          </p>
          <div className="hero__actions">
            <Link className="button button--primary" to="/coins">
              Explore markets
            </Link>
          </div>
        </div>
      </section>

      <section className="container page-section">
        <header className="section-header section-header--stacked">
          <div>
            <h2>What Inferra does</h2>
            <p>Four core capabilities working together to give you a sharper view of the crypto market.</p>
          </div>
        </header>

        <div className="about-features">
          {features.map((f) => (
            <article key={f.title} className="surface-card about-feature-card">
              <h3>{f.title}</h3>
              <p>{f.description}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="container page-section">
        <header className="section-header section-header--stacked">
          <div>
            <h2>Built with</h2>
            <p>Open data sources and proven technologies power every part of the platform.</p>
          </div>
        </header>

        <div className="about-stack">
          {techStack.map((t) => (
            <div key={t.name} className="surface-card about-stack-item">
              <strong>{t.name}</strong>
              <span>{t.role}</span>
            </div>
          ))}
        </div>
      </section>

      <section className="container page-section">
        <article className="surface-card about-thesis">
          <div className="eyebrow">Background</div>
          <h2>University thesis project</h2>
          <p>
            Inferra was built as a diploma thesis exploring the intersection of financial data
            engineering and applied machine learning. The goal: prove that accessible, real-time
            crypto analysis doesn't require expensive data vendors or black-box tools.
          </p>
        </article>
      </section>
    </div>
  );
}

export default AboutPage;
