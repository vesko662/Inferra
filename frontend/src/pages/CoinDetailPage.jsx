import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import ErrorMessage from "../components/common/ErrorMessage";
import LoadingState from "../components/common/LoadingState";
import CandleChart from "../components/crypto/CandleChart";
import DetailField from "../components/crypto/DetailField";
import PricePanel from "../components/crypto/PricePanel";
import SentimentGauge from "../components/crypto/SentimentGauge";
import useAssetCandles from "../hooks/useAssetCandles";
import useAssetForecast from "../hooks/useAssetForecast";
import useAssetSentiment from "../hooks/useAssetSentiment";
import useCoinDetails from "../hooks/useCoinDetails";
import { getAssetDisplayLabel, getAssetRouteKey } from "../utils/assetMappers";
import { formatCompactNumber, formatDateTime, formatRelativeTime } from "../utils/formatters";

const chartRanges = [
  { value: "all", label: "All" },
  { value: 90, label: "90d" },
  { value: 30, label: "30d" },
  { value: 7, label: "7d" }
];
const chartModes = [
  { value: "simple", label: "Simple" },
  { value: "advanced", label: "Advanced" }
];

function CoinDetailPage() {
  const { symbol } = useParams();
  const [selectedRange, setSelectedRange] = useState("all");
  const [chartMode, setChartMode] = useState("simple");
  const { asset, snapshot, isLoading, error, lastUpdated, refresh } = useCoinDetails(symbol);
  const {
    candles,
    error: candlesError,
    isLoading: candlesLoading
  } = useAssetCandles(symbol, selectedRange);
  const { forecast } = useAssetForecast(symbol);
  const { isAuthenticated, login } = useAuth();
  const { sentiment, error: sentimentError, isLoading: sentimentLoading } = useAssetSentiment(isAuthenticated ? symbol : null);

  if (isLoading) {
    return (
      <div className="container page-section">
        <LoadingState label="Loading coin details..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="container page-section">
        <ErrorMessage
          title="Unable to load coin details"
          message={error}
          actionLabel="Try again"
          onAction={refresh}
        />
      </div>
    );
  }

  if (!asset) {
    return (
      <div className="container page-section">
        <ErrorMessage
          title="Asset not found"
          message="The requested asset could not be found."
        />
      </div>
    );
  }

  const assetLabel = getAssetDisplayLabel(asset);
  const routeKey = getAssetRouteKey(asset) || symbol;

  return (
    <div className="page">
      <section className="container page-heading page-heading--detail">
        <div>
          <Link className="text-link" to="/coins">
            Back to markets
          </Link>
          <h1>{assetLabel}</h1>
          <p>
            A dedicated asset page that keeps current market snapshot data separate from historical
            trend visualization.
          </p>
        </div>

        <div className="page-heading__meta">
          <span className="meta-chip">Last refreshed {formatRelativeTime(lastUpdated)}</span>
          <button className="button button--ghost" type="button" onClick={refresh}>
            Refresh now
          </button>
        </div>
      </section>

      <section className="container detail-grid">
        <PricePanel snapshot={snapshot} />

        <article className="surface-card detail-card">
          <header className="section-header">
            <div>
              <h2>Asset details</h2>
              <p>Core asset information and supply metrics.</p>
            </div>
          </header>

          <div className="asset-overview">
            {asset.imageUrl ? (
              <img className="asset-overview__logo" src={asset.imageUrl} alt={`${assetLabel} logo`} />
            ) : (
              <div className="asset-overview__logo asset-overview__logo--fallback" aria-hidden="true">
                {(routeKey || "?").slice(0, 1)}
              </div>
            )}

            <div>
              <h2 className="asset-overview__title">{assetLabel}</h2>
              <p className="asset-overview__meta">
                {asset.symbol || "No symbol"} {asset.pairSymbol ? `| ${asset.pairSymbol}` : ""}
              </p>
            </div>
          </div>

          <div className="detail-fields">
            <DetailField label="Circulating supply" value={formatCompactNumber(asset.circulatingSupply)} />
            <DetailField label="Total supply" value={formatCompactNumber(asset.totalSupply)} />
            <DetailField label="Max supply" value={formatCompactNumber(asset.maxSupply)} />
            <DetailField
              label="Snapshot timestamp"
              value={snapshot?.updatedAt ? formatDateTime(snapshot.updatedAt) : "N/A"}
            />
          </div>
        </article>

        <article className="surface-card detail-card">
          <header className="section-header">
            <div>
              <h2>News sentiment</h2>
              <p>Aggregated bullish / bearish signal from recent news articles.</p>
            </div>
          </header>

          {!isAuthenticated ? (
            <div className="sentiment-locked">
              <div className="sentiment-locked__blur" aria-hidden="true">
                <SentimentGauge sentiment={{ bullish: 40, bearish: 30, neutral: 30, generatedAt: null }} />
              </div>
              <div className="sentiment-locked__overlay">
                <p>Log in to view sentiment data</p>
                <button className="button button--primary" type="button" onClick={login}>
                  Log in
                </button>
              </div>
            </div>
          ) : (
            <>
              {sentimentLoading && <LoadingState label="Loading sentiment..." />}
              {!sentimentLoading && sentimentError && (
                <div className="sentiment-locked">
                  <div className="sentiment-locked__blur" aria-hidden="true">
                    <SentimentGauge sentiment={{ bullish: 40, bearish: 30, neutral: 30, generatedAt: null }} />
                  </div>
                  <div className="sentiment-locked__overlay">
                    <p>This asset is not yet supported</p>
                  </div>
                </div>
              )}
              {!sentimentLoading && !sentimentError && !sentiment && (
                <p style={{ color: "var(--color-text-muted)", fontSize: "0.875rem" }}>
                  No sentiment data available yet.
                </p>
              )}
              {!sentimentLoading && !sentimentError && sentiment && <SentimentGauge sentiment={sentiment} />}
            </>
          )}
        </article>

        <article className="surface-card detail-card detail-card--wide">
          <header className="section-header">
            <div>
              <h2>Price history</h2>
              <p>Use a simple line for fast reading or switch to candlesticks for OHLC detail.</p>
            </div>

            <div className="section-header__actions">
              {chartModes.map((mode) => (
                <button
                  key={mode.value}
                  className={mode.value === chartMode ? "button button--primary" : "button button--ghost"}
                  type="button"
                  onClick={() => setChartMode(mode.value)}
                >
                  {mode.label}
                </button>
              ))}
              {chartRanges.map((range) => (
                <button
                  key={range.value}
                  className={range.value === selectedRange ? "button button--primary" : "button button--ghost"}
                  type="button"
                  onClick={() => setSelectedRange(range.value)}
                >
                  {range.label}
                </button>
              ))}
            </div>
          </header>

          {candlesLoading ? <LoadingState label="Loading historical candles..." /> : null}
          {!candlesLoading && candlesError ? (
            <ErrorMessage title="Unable to load historical chart data" message={candlesError} />
          ) : null}
          {!candlesLoading && !candlesError ? (
            <CandleChart
              candles={candles}
              snapshot={snapshot}
              forecastModels={isAuthenticated ? forecast : []}
              mode={chartMode}
            />
          ) : null}

          {!isAuthenticated && (
            <div className="forecast-locked">
              <div className="forecast-locked__preview" aria-hidden="true">
                {[
                  { label: "LinearRegression", color: "#f4c96b" },
                  { label: "LSTM", color: "#c48bff" },
                  { label: "XGBoost", color: "#ff8a65" }
                ].map((m) => (
                  <div key={m.label} className="forecast-locked__model">
                    <span className="forecast-locked__swatch" style={{ backgroundColor: m.color }} />
                    <span className="forecast-locked__model-label">{"█".repeat(m.label.length)}</span>
                  </div>
                ))}
              </div>
              <div className="forecast-locked__cta">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
                  <path d="M7 11V7a5 5 0 0 1 10 0v4" />
                </svg>
                <span>Log in to unlock price predictions</span>
                <button className="button button--primary button--sm" type="button" onClick={login}>
                  Log in
                </button>
              </div>
            </div>
          )}
        </article>

      </section>
    </div>
  );
}

export default CoinDetailPage;
