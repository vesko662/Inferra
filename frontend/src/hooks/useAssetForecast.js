import { useEffect, useState } from "react";
import { getLatestAssetForecast } from "../services/cryptoService";

function useAssetForecast(symbol) {
  const [forecast, setForecast] = useState([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isCancelled = false;

    async function loadForecast() {
      setIsLoading(true);
      setError("");

      try {
        const response = await getLatestAssetForecast(symbol);

        if (!isCancelled) {
          setForecast(response);
        }
      } catch (loadError) {
        if (!isCancelled) {
          setError(loadError.message || "Unable to load forecast data.");
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false);
        }
      }
    }

    if (symbol) {
      loadForecast();
    }

    return () => {
      isCancelled = true;
    };
  }, [symbol]);

  return {
    forecast,
    error,
    isLoading
  };
}

export default useAssetForecast;
