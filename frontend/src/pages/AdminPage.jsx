import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import ErrorMessage from "../components/common/ErrorMessage";
import LoadingState from "../components/common/LoadingState";
import SearchInput from "../components/common/SearchInput";
import { getAssets } from "../services/cryptoService";
import {
  deleteAsset,
  setMlEnabled,
  setNewsEnabled,
  triggerDailyPrediction,
  triggerSentiment,
  triggerTraining
} from "../services/adminService";

const JOBS = [
  { key: "training", label: "Training", trigger: triggerTraining },
  { key: "prediction", label: "Daily prediction", trigger: triggerDailyPrediction },
  { key: "sentiment", label: "News sentiment", trigger: triggerSentiment }
];

function AdminPage() {
  const { isAdmin, isInitialized } = useAuth();
  const navigate = useNavigate();
  const [assets, setAssets] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [query, setQuery] = useState("");
  const [togglePending, setTogglePending] = useState({});
  const [localFlags, setLocalFlags] = useState({});
  const [jobStates, setJobStates] = useState({});
  const [deletePending, setDeletePending] = useState({});

  const filteredAssets = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return assets;
    return assets.filter(
      (a) =>
        a.name?.toLowerCase().includes(q) ||
        a.symbol?.toLowerCase().includes(q)
    );
  }, [assets, query]);

  const loadAssets = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await getAssets();
      setAssets(data);
    } catch (err) {
      setError(err.message || "Unable to load assets.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    if (isAdmin) loadAssets();
  }, [isAdmin, loadAssets]);

  useEffect(() => {
    if (isInitialized && !isAdmin) {
      navigate("/", { replace: true });
    }
  }, [isAdmin, isInitialized, navigate]);

  useEffect(() => {
    if (assets.length > 0) {
      const initial = {};
      assets.forEach((a) => {
        initial[a.symbol] = { isMlEnabled: a.isMlEnabled, isNewsEnabled: a.isNewsEnabled };
      });
      setLocalFlags((prev) => ({ ...initial, ...prev }));
    }
  }, [assets]);

  async function handleToggle(symbol, field, value) {
    const key = `${symbol}_${field}`;
    setTogglePending((p) => ({ ...p, [key]: true }));
    try {
      if (field === "isMlEnabled") {
        await setMlEnabled(symbol, value);
      } else {
        await setNewsEnabled(symbol, value);
      }
      setLocalFlags((prev) => ({
        ...prev,
        [symbol]: { ...prev[symbol], [field]: value }
      }));
    } catch {
      // revert on failure — do nothing, localFlags stays unchanged
    } finally {
      setTogglePending((p) => ({ ...p, [key]: false }));
    }
  }

  async function handleDelete(symbol) {
    if (!window.confirm(`Delete ${symbol}? This action cannot be undone.`)) return;
    setDeletePending((p) => ({ ...p, [symbol]: true }));
    try {
      await deleteAsset(symbol);
      setAssets((prev) => prev.filter((a) => a.symbol !== symbol));
    } catch {
      // keep asset in list on failure
    } finally {
      setDeletePending((p) => ({ ...p, [symbol]: false }));
    }
  }

  async function handleJobTrigger(job) {
    setJobStates((s) => ({ ...s, [job.key]: { loading: true, message: null, error: null } }));
    try {
      await job.trigger();
      setJobStates((s) => ({
        ...s,
        [job.key]: { loading: false, message: "Triggered successfully.", error: null }
      }));
    } catch (err) {
      setJobStates((s) => ({
        ...s,
        [job.key]: { loading: false, message: null, error: err.message || "Failed to trigger." }
      }));
    }
  }

  if (!isInitialized) {
    return (
      <div className="container page-section">
        <LoadingState label="Loading..." />
      </div>
    );
  }

  if (!isAdmin) return null;

  return (
    <div className="page">
      <section className="container page-heading">
        <div>
          <div className="eyebrow">Admin</div>
          <h1>Admin panel</h1>
          <p>Manage asset flags and trigger ML pipelines.</p>
        </div>
      </section>

      <section className="container page-section">
        <h2>Jobs</h2>
        <div className="surface-card" style={{ marginTop: "1rem" }}>
          {JOBS.map((job) => {
            const state = jobStates[job.key];
            return (
              <div
                key={job.key}
                style={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  padding: "0.875rem 1.25rem",
                  borderBottom: "0.5px solid var(--color-border)"
                }}
              >
                <span style={{ fontWeight: 500 }}>{job.label}</span>
                <div style={{ display: "flex", alignItems: "center", gap: "0.75rem" }}>
                  {state?.loading && <span style={{ fontSize: "0.875rem", opacity: 0.6 }}>Running...</span>}
                  {state?.message && <span style={{ fontSize: "0.875rem", color: "var(--color-success)" }}>{state.message}</span>}
                  {state?.error && <span style={{ fontSize: "0.875rem", color: "var(--color-danger)" }}>{state.error}</span>}
                  <button
                    className="button button--ghost"
                    type="button"
                    disabled={state?.loading}
                    onClick={() => handleJobTrigger(job)}
                  >
                    Run
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      </section>

      <section className="container page-section">
        <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "1rem" }}>
          <h2>Assets</h2>
          <SearchInput value={query} onChange={setQuery} placeholder="Search assets..." />
        </div>

        {isLoading && <LoadingState label="Loading assets..." />}
        {!isLoading && error && <ErrorMessage title="Unable to load assets" message={error} />}

        {!isLoading && !error && (
          <div className="surface-card">
            <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "0.9rem" }}>
              <thead>
                <tr style={{ borderBottom: "0.5px solid var(--color-border)" }}>
                  <th style={{ textAlign: "left", padding: "0.75rem 1.25rem", fontWeight: 500 }}>Asset</th>
                  <th style={{ textAlign: "center", padding: "0.75rem 1.25rem", fontWeight: 500 }}>ML</th>
                  <th style={{ textAlign: "center", padding: "0.75rem 1.25rem", fontWeight: 500 }}>News</th>
                  <th style={{ padding: "0.75rem 1.25rem" }}></th>
                </tr>
              </thead>
              <tbody>
                {filteredAssets.map((asset) => {
                  const flags = localFlags[asset.symbol] ?? { isMlEnabled: asset.isMlEnabled, isNewsEnabled: asset.isNewsEnabled };
                  return (
                    <tr key={asset.symbol} style={{ borderBottom: "0.5px solid var(--color-border)" }}>
                      <td style={{ padding: "0.75rem 1.25rem" }}>
                        <div style={{ display: "flex", alignItems: "center", gap: "0.75rem" }}>
                          {asset.imageUrl ? (
                            <img
                              src={asset.imageUrl}
                              alt={asset.name || asset.symbol}
                              style={{ width: 28, height: 28, borderRadius: "50%", objectFit: "contain" }}
                            />
                          ) : (
                            <div style={{
                              width: 28, height: 28, borderRadius: "50%",
                              background: "var(--color-background-secondary)",
                              display: "flex", alignItems: "center", justifyContent: "center",
                              fontSize: "0.75rem", fontWeight: 500, opacity: 0.6
                            }}>
                              {(asset.symbol || "?").slice(0, 1)}
                            </div>
                          )}
                          <div>
                            <div style={{ fontWeight: 500 }}>{asset.name || asset.symbol}</div>
                            <div style={{ fontSize: "0.8rem", opacity: 0.6 }}>{asset.symbol}</div>
                          </div>
                        </div>
                      </td>
                      <td style={{ textAlign: "center", padding: "0.75rem 1.25rem" }}>
                        <input
                          type="checkbox"
                          checked={flags.isMlEnabled}
                          disabled={!!togglePending[`${asset.symbol}_isMlEnabled`]}
                          onChange={(e) => handleToggle(asset.symbol, "isMlEnabled", e.target.checked)}
                        />
                      </td>
                      <td style={{ textAlign: "center", padding: "0.75rem 1.25rem" }}>
                        <input
                          type="checkbox"
                          checked={flags.isNewsEnabled}
                          disabled={!!togglePending[`${asset.symbol}_isNewsEnabled`]}
                          onChange={(e) => handleToggle(asset.symbol, "isNewsEnabled", e.target.checked)}
                        />
                      </td>
                      <td style={{ padding: "0.75rem 1.25rem", textAlign: "right" }}>
                        <button
                          className="button button--ghost"
                          type="button"
                          disabled={!!deletePending[asset.symbol]}
                          onClick={() => handleDelete(asset.symbol)}
                          style={{ color: "var(--color-danger, #e24b4a)", borderColor: "var(--color-danger, #e24b4a)" }}
                        >
                          {deletePending[asset.symbol] ? "Deleting..." : "Delete"}
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}

export default AdminPage;
