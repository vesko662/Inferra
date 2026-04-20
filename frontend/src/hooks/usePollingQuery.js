import { useCallback, useEffect, useRef, useState } from "react";
import { POLLING_INTERVAL_MS } from "../config/appConfig";

function usePollingQuery(fetcher, dependencies = []) {
  const [data, setData] = useState(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [lastUpdated, setLastUpdated] = useState("");
  const intervalRef = useRef(null);
  const fetcherRef = useRef(fetcher);

  useEffect(() => {
    fetcherRef.current = fetcher;
  }, [fetcher]);

  const runQuery = useCallback(async () => {
    setError("");

    try {
      const response = await fetcherRef.current();
      setData(response);
      setLastUpdated(new Date().toISOString());
    } catch (queryError) {
      setError(queryError.message || "Something went wrong while loading data.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    setIsLoading(true);
    runQuery();

    intervalRef.current = window.setInterval(() => {
      runQuery();
    }, POLLING_INTERVAL_MS);

    return () => {
      if (intervalRef.current) {
        window.clearInterval(intervalRef.current);
      }
    };
  }, [runQuery, ...dependencies]);

  return {
    data,
    error,
    isLoading,
    lastUpdated,
    refresh: runQuery
  };
}

export default usePollingQuery;
