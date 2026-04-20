import { useCallback, useEffect, useMemo, useState } from "react";
import { POLLING_INTERVAL_MS } from "../config/appConfig";
import { getAssets, getLatestAssetSnapshot } from "../services/cryptoService";
import { getAssetSearchText, mergeAssetWithSnapshot } from "../utils/assetMappers";
import { formatRelativeTime } from "../utils/formatters";
import usePollingQuery from "./usePollingQuery";

function useAssets({ limit, page = 1, pageSize = 10, enablePagination = false } = {}) {
  const [query, setQuery] = useState("");
  const [snapshotMap, setSnapshotMap] = useState({});
  const [isSnapshotsLoading, setIsSnapshotsLoading] = useState(false);
  const [snapshotError, setSnapshotError] = useState("");
  const [snapshotsUpdatedAt, setSnapshotsUpdatedAt] = useState("");
  const { data, error, isLoading, lastUpdated, refresh } = usePollingQuery(
    () => getAssets(),
    [limit]
  );

  const assets = useMemo(() => {
    const items = Array.isArray(data) ? data : [];
    return limit ? items.slice(0, limit) : items;
  }, [data, limit]);

  const filteredAssets = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    if (!normalizedQuery) {
      return assets;
    }

    return assets.filter((asset) => getAssetSearchText(asset).includes(normalizedQuery));
  }, [assets, query]);

  const visibleBaseAssets = useMemo(() => {
    if (!enablePagination) {
      return filteredAssets;
    }

    const startIndex = (page - 1) * pageSize;
    return filteredAssets.slice(startIndex, startIndex + pageSize);
  }, [enablePagination, filteredAssets, page, pageSize]);

  const visibleSymbolsKey = useMemo(
    () => visibleBaseAssets.map((asset) => asset.symbol).filter(Boolean).join("|"),
    [visibleBaseAssets]
  );

  const loadVisibleSnapshots = useCallback(
    async ({ force = false } = {}) => {
      const symbols = visibleBaseAssets.map((asset) => asset.symbol).filter(Boolean);

      if (!symbols.length) {
        setSnapshotError("");
        setIsSnapshotsLoading(false);
        return;
      }

      const shouldFetch = force || symbols.some((symbol) => !snapshotMap[symbol]);

      if (!shouldFetch) {
        return;
      }

      setIsSnapshotsLoading(true);
      setSnapshotError("");

      try {
        const responses = await Promise.all(
          symbols.map(async (symbol) => {
            try {
              const snapshot = await getLatestAssetSnapshot(symbol);
              return [symbol, snapshot];
            } catch {
              return [symbol, null];
            }
          })
        );

        setSnapshotMap((currentSnapshots) => {
          const nextSnapshots = { ...currentSnapshots };

          responses.forEach(([symbol, snapshot]) => {
            nextSnapshots[symbol] = snapshot;
          });

          return nextSnapshots;
        });
        setSnapshotsUpdatedAt(new Date().toISOString());
      } catch (loadError) {
        setSnapshotError(loadError.message || "Unable to load market snapshots.");
      } finally {
        setIsSnapshotsLoading(false);
      }
    },
    [snapshotMap, visibleBaseAssets]
  );

  useEffect(() => {
    loadVisibleSnapshots();
  }, [loadVisibleSnapshots, visibleSymbolsKey]);

  useEffect(() => {
    const intervalId = window.setInterval(() => {
      loadVisibleSnapshots({ force: true });
    }, POLLING_INTERVAL_MS);

    return () => {
      window.clearInterval(intervalId);
    };
  }, [loadVisibleSnapshots]);

  const visibleAssets = useMemo(
    () =>
      visibleBaseAssets.map((asset) =>
        mergeAssetWithSnapshot(asset, asset.symbol ? snapshotMap[asset.symbol] : null)
      ),
    [snapshotMap, visibleBaseAssets]
  );

  const refreshVisibleAssets = useCallback(async () => {
    await refresh();
    await loadVisibleSnapshots({ force: true });
  }, [loadVisibleSnapshots, refresh]);

  return {
    assets,
    filteredAssets,
    visibleAssets,
    query,
    setQuery,
    error: error || snapshotError,
    isLoading: isLoading || isSnapshotsLoading,
    lastUpdated: formatRelativeTime(snapshotsUpdatedAt || lastUpdated),
    refresh: refreshVisibleAssets
  };
}

export default useAssets;
