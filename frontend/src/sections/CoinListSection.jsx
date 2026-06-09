import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import EmptyState from "../components/common/EmptyState";
import ErrorMessage from "../components/common/ErrorMessage";
import LoadingState from "../components/common/LoadingState";
import SearchInput from "../components/common/SearchInput";
import AssetRow from "../components/crypto/AssetRow";
import useAssets from "../hooks/useAssets";

function CoinListSection({
  title,
  description,
  limit,
  showViewAll = false,
  showSearch = true,
  showRefresh = true,
  showLastUpdated = true,
  pageSize = 10,
  enablePagination = false
}) {
  const [currentPage, setCurrentPage] = useState(1);

  const {
    filteredAssets,
    visibleAssets,
    query,
    setQuery,
    isLoading,
    error,
    lastUpdated,
    refresh
  } = useAssets({
    limit,
    page: currentPage,
    pageSize,
    enablePagination
  });

  const totalPages = useMemo(() => {
    if (!enablePagination) {
      return 1;
    }

    return Math.max(1, Math.ceil(filteredAssets.length / pageSize));
  }, [enablePagination, filteredAssets.length, pageSize]);

  useEffect(() => {
    setCurrentPage(1);
  }, [query]);

  useEffect(() => {
    if (currentPage > totalPages) {
      setCurrentPage(totalPages);
    }
  }, [currentPage, totalPages]);

  return (
    <div className="surface-card section-card">
      <header className="section-header section-header--split">
        <div>
          <h2>{title}</h2>
          <p>{description}</p>
        </div>

        <div className="section-header__actions">
          {showLastUpdated ? (
            <span className="meta-chip">
              {lastUpdated ? `Updated ${lastUpdated}` : "Awaiting data"}
            </span>
          ) : null}
          {showRefresh ? (
            <button className="button button--ghost" type="button" onClick={refresh}>
              Refresh
            </button>
          ) : null}
          {showViewAll ? (
            <Link className="button button--primary" to="/coins">
              View all
            </Link>
          ) : null}
        </div>
      </header>

      {showSearch ? (
        <SearchInput
          label="Search assets"
          placeholder="Search by asset name, symbol, or pair"
          value={query}
          onChange={setQuery}
        />
      ) : null}

      {isLoading ? <LoadingState label="Loading tracked assets..." /> : null}

      {!isLoading && error ? (
        <ErrorMessage
          title="Unable to load assets"
          message={error}
          actionLabel="Retry"
          onAction={refresh}
        />
      ) : null}

      {!isLoading && !error && filteredAssets.length === 0 ? (
        <EmptyState
          title="No assets found"
          message={
            query
              ? "Try a different search term."
              : "The backend returned no asset records."
          }
        />
      ) : null}

      {!isLoading && !error && filteredAssets.length > 0 ? (
        <div className="market-table" role="list" aria-label={title}>
          <div className="market-table__header" aria-hidden="true">
            <span>Asset</span>
            <span>Price</span>
            <span>24h</span>
            <span>Volume</span>
          </div>
          {visibleAssets.map((asset, index) => (
            <AssetRow
              key={asset.symbol || `asset-${index}`}
              asset={asset}
              index={enablePagination ? (currentPage - 1) * pageSize + index : index}
            />
          ))}
        </div>
      ) : null}

      {!isLoading && !error && enablePagination && filteredAssets.length > 0 ? (
        <div className="pagination">
          <button
            className="button button--ghost"
            type="button"
            onClick={() => setCurrentPage((page) => Math.max(1, page - 1))}
            disabled={currentPage === 1}
          >
            Previous
          </button>

          <span className="pagination__status">
            Page {currentPage} of {totalPages}
          </span>

          <button
            className="button button--ghost"
            type="button"
            onClick={() => setCurrentPage((page) => Math.min(totalPages, page + 1))}
            disabled={currentPage === totalPages}
          >
            Next
          </button>
        </div>
      ) : null}

      {!isLoading && !error ? (
        <p className="section-footnote">
          The market overview combines the asset list with the latest snapshot endpoint so prices,
          24h change, market cap, and volume stay current.
        </p>
      ) : null}
    </div>
  );
}

export default CoinListSection;
