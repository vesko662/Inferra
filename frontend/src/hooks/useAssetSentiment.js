import { useEffect, useState } from "react";
import { getAssetSentiment } from "../services/cryptoService";

function useAssetSentiment(symbol) {
  const [sentiment, setSentiment] = useState(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isCancelled = false;

    async function loadSentiment() {
      setIsLoading(true);
      setError("");

      try {
        const response = await getAssetSentiment(symbol);

        if (!isCancelled) {
          setSentiment(response);
        }
      } catch (loadError) {
        if (!isCancelled) {
          setError(loadError.message || "Unable to load sentiment data.");
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false);
        }
      }
    }

    if (symbol) {
      loadSentiment();
    }

    return () => {
      isCancelled = true;
    };
  }, [symbol]);

  return { sentiment, error, isLoading };
}

export default useAssetSentiment;
