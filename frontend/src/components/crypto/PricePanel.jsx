import { formatCompactNumber, formatCurrency, formatDateTime, formatPercentage } from "../../utils/formatters";

function PricePanel({ snapshot }) {
  const changeClassName =
    snapshot?.priceChangePercentage24h > 0
      ? "trend trend--positive"
      : snapshot?.priceChangePercentage24h < 0
        ? "trend trend--negative"
        : "trend";

  return (
    <article className="surface-card detail-card detail-card--price">
      <header className="section-header">
        <div>
          <h2>Latest snapshot</h2>
          <p>Fresh market values for the selected asset.</p>
        </div>
      </header>

      {snapshot ? (
        <div className="snapshot-grid">
          <div className="snapshot-hero">
            <strong>{formatCurrency(snapshot.price)}</strong>
            <span className={changeClassName}>
              {formatPercentage(snapshot.priceChangePercentage24h)} in 24h
            </span>
          </div>

          <div className="detail-fields">
            <div className="detail-field">
              <span>Price change 24h</span>
              <strong>{formatCurrency(snapshot.priceChange24h)}</strong>
            </div>
            <div className="detail-field">
              <span>Volume 24h</span>
              <strong>{formatCompactNumber(snapshot.volume24h)}</strong>
            </div>
            <div className="detail-field">
              <span>Updated at</span>
              <strong>{formatDateTime(snapshot.updatedAt)}</strong>
            </div>
          </div>
        </div>
      ) : (
        <p className="section-footnote">No latest snapshot values were returned for this asset.</p>
      )}
    </article>
  );
}

export default PricePanel;
