import { Link } from "react-router-dom";
import { getAssetDisplayLabel, getAssetRouteKey } from "../../utils/assetMappers";
import { formatCompactNumber, formatCurrency, formatPercentage } from "../../utils/formatters";

function AssetRow({ asset, index }) {
  const routeKey = getAssetRouteKey(asset);
  const changeValue = asset.snapshot?.priceChangePercentage24h;
  const changeClassName =
    changeValue > 0 ? "trend trend--positive" : changeValue < 0 ? "trend trend--negative" : "trend";
  const content = (
    <>
      <div className="market-row__asset">
        <span className="market-row__rank">{index + 1}</span>
        {asset.imageUrl ? (
          <img className="market-row__logo" src={asset.imageUrl} alt="" />
        ) : (
          <div className="market-row__logo market-row__logo--fallback" aria-hidden="true">
            {(asset.symbol || "?").slice(0, 1)}
          </div>
        )}
        <div>
          <strong>{getAssetDisplayLabel(asset, index)}</strong>
          <div className="market-row__subline">
            <span>{asset.symbol || "N/A"}</span>
          </div>
        </div>
      </div>

      <div className="market-row__cell market-row__cell--price">
        <span className="market-row__label">Price</span>
        <strong>{formatCurrency(asset.snapshot?.price)}</strong>
      </div>

      <div className="market-row__cell">
        <span className="market-row__label">24h</span>
        <strong className={changeClassName}>{formatPercentage(changeValue)}</strong>
      </div>

      <div className="market-row__cell">
        <span className="market-row__label">Volume</span>
        <strong>{formatCompactNumber(asset.snapshot?.volume24h)}</strong>
      </div>
    </>
  );

  if (!routeKey) {
    return <div className="market-row market-row--disabled">{content}</div>;
  }

  return (
    <Link className="market-row" to={`/coins/${encodeURIComponent(routeKey)}`}>
      {content}
    </Link>
  );
}

export default AssetRow;
