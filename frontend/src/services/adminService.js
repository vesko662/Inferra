import httpClient from "./httpClient";

export async function setMlEnabled(symbol, enabled) {
  return httpClient(
    `/api/admin/assets/${encodeURIComponent(symbol)}/ml-enabled?enabled=${enabled}`,
    { method: "PATCH" }
  );
}

export async function setNewsEnabled(symbol, enabled) {
  return httpClient(
    `/api/admin/assets/${encodeURIComponent(symbol)}/news-enabled?enabled=${enabled}`,
    { method: "PATCH" }
  );
}

export async function triggerTraining() {
  return httpClient("/api/admin/ml/train", { method: "POST" });
}

export async function triggerDailyPrediction() {
  return httpClient("/api/admin/ml/predict-daily", { method: "POST" });
}

export async function triggerSentiment() {
  return httpClient("/api/admin/ml/sentiment", { method: "POST" });
}

export async function deleteAsset(symbol) {
  return httpClient(`/api/admin/assets/${encodeURIComponent(symbol)}`, { method: "DELETE" });
}
