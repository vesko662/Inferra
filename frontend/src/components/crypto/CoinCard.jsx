import { Link } from "react-router-dom";
import { getAssetDisplayLabel, getAssetRouteKey } from "../../utils/assetMappers";
import {
  formatCompactNumber,
  formatCurrency,
  formatPercentage,
  formatRelativeTime
} from "../../utils/formatters";

function CoinCard({ asset, index }) {
  const routeKey = getAssetRouteKey(asset);
  const priceChange = asset.snapshot?.priceChangePercentage24h;
  const changeClassName =
    priceChange > 0 ? "trend trend--positive" : priceChange < 0 ? "trend trend--negative" : "trend";

  return (
    <article className="coin-card" role="listitem">
      <div className="coin-card__top">
        <div className="coin-card__identity">
          {asset.imageUrl ? (
            <img className="coin-card__logo" src={asset.imageUrl} alt={`${asset.name || asset.symbol} logo`} />
          ) : (
            <div className="coin-card__logo coin-card__logo--fallback" aria-hidden="true">
              {(asset.symbol || "?").slice(0, 1)}
            </div>
          )}

          <div>
            <div className="coin-card__symbol">{asset.symbol || `Asset ${index + 1}`}</div>
            <h3>{getAssetDisplayLabel(asset, index)}</h3>
            <p className="coin-card__pair">{asset.pairSymbol || "Trading pair unavailable"}</p>
          </div>
        </div>
        <span className="coin-card__rank">{asset.id ? `#${asset.id}` : "N/A"}</span>
      </div>

      <div className="coin-card__price">
        {asset.snapshot ? formatCurrency(asset.snapshot.price) : "Open details for live price"}
      </div>

      <div className="coin-card__meta">
        <span className={changeClassName}>
          {asset.snapshot ? formatPercentage(priceChange) : "Snapshot on detail page"}
        </span>
        <span>{asset.snapshot?.updatedAt ? formatRelativeTime(asset.snapshot.updatedAt) : asset.pairSymbol || "No pair"}</span>
      </div>

      <div className="coin-card__stats">
        <div className="preview-line">
          <span>Trading pair</span>
          <strong>{asset.pairSymbol || "N/A"}</strong>
        </div>
        <div className="preview-line">
          <span>Market cap</span>
          <strong>{asset.snapshot ? formatCompactNumber(asset.snapshot.marketCap) : "Load details"}</strong>
        </div>
      </div>

      {routeKey ? (
        <Link className="text-link" to={`/coins/${encodeURIComponent(routeKey)}`}>
          View details
        </Link>
      ) : (
        <span className="text-link text-link--muted">Detail route unavailable without a symbol</span>
      )}
    </article>
  );
}

export default CoinCard;
