import { useEffect, useMemo, useState } from "react";
import { getAssetCandles } from "../services/cryptoService";

function buildDateRange(days) {
  if (!days || days === "all") {
    return {};
  }

  const toDate = new Date();
  const fromDate = new Date();
  fromDate.setDate(toDate.getDate() - days);

  return {
    from: fromDate.toISOString().slice(0, 10),
    to: toDate.toISOString().slice(0, 10)
  };
}

function useAssetCandles(symbol, rangeInDays = "all") {
  const [candles, setCandles] = useState([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  const range = useMemo(() => buildDateRange(rangeInDays), [rangeInDays]);

  useEffect(() => {
    let isCancelled = false;

    async function loadCandles() {
      setIsLoading(true);
      setError("");

      try {
        const response = await getAssetCandles(symbol, range);

        if (!isCancelled) {
          setCandles(response);
        }
      } catch (loadError) {
        if (!isCancelled) {
          setError(loadError.message || "Unable to load historical data.");
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false);
        }
      }
    }

    if (symbol) {
      loadCandles();
    }

    return () => {
      isCancelled = true;
    };
  }, [symbol, range]);

  return {
    candles,
    error,
    isLoading,
    range
  };
}

export default useAssetCandles;
