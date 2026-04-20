import { getAssetBySymbol, getLatestAssetSnapshot } from "../services/cryptoService";
import usePollingQuery from "./usePollingQuery";

function useCoinDetails(symbol) {
  const { data, error, isLoading, lastUpdated, refresh } = usePollingQuery(
    async () => {
      const [asset, snapshot] = await Promise.all([
        getAssetBySymbol(symbol),
        getLatestAssetSnapshot(symbol)
      ]);

      return { asset, snapshot: snapshot || asset.snapshot };
    },
    [symbol]
  );

  return {
    asset: data?.asset ?? null,
    snapshot: data?.snapshot ?? null,
    error,
    isLoading,
    lastUpdated,
    refresh
  };
}

export default useCoinDetails;
