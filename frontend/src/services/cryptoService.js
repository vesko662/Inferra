import { API_ENDPOINTS } from "../config/api";
import {
  normalizeCandle,
  normalizeAssetDetails,
  normalizeAssetListItem,
  normalizeForecast,
  normalizeSnapshot
} from "../utils/assetMappers";
import httpClient from "./httpClient";

export async function getAssets() {
  const response = await httpClient(API_ENDPOINTS.assets);
  return Array.isArray(response) ? response.map(normalizeAssetListItem) : [];
}

export async function getAssetBySymbol(symbol) {
  if (!symbol) {
    throw new Error("A symbol is required.");
  }

  const response = await httpClient(`${API_ENDPOINTS.assets}/${encodeURIComponent(symbol)}`);
  return normalizeAssetDetails(response);
}

export async function getLatestAssetSnapshot(symbol) {
  if (!symbol) {
    throw new Error("A symbol is required.");
  }

  const response = await httpClient(`${API_ENDPOINTS.assets}/${encodeURIComponent(symbol)}/snapshots/latest`);
  return normalizeSnapshot(response);
}

export async function getAssetCandles(symbol, options = {}) {
  if (!symbol) {
    throw new Error("A symbol is required.");
  }

  const searchParams = new URLSearchParams();

  if (options.from) {
    searchParams.set("from", options.from);
  }

  if (options.to) {
    searchParams.set("to", options.to);
  }

  const queryString = searchParams.toString();
  const path = `${API_ENDPOINTS.assets}/${encodeURIComponent(symbol)}/candles${queryString ? `?${queryString}` : ""}`;
  const response = await httpClient(path);

  return Array.isArray(response) ? response.map(normalizeCandle).filter(Boolean) : [];
}

export async function getAssetSentiment(symbol) {
  if (!symbol) {
    throw new Error("A symbol is required.");
  }

  const response = await httpClient(
    `${API_ENDPOINTS.assets}/${encodeURIComponent(symbol)}/sentiment`
  );

  return {
    bullish: response.bullish ?? 0,
    bearish: response.bearish ?? 0,
    neutral: response.neutral ?? 0,
    generatedAt: response.generatedAt ?? null
  };
}

export async function getLatestAssetForecast(symbol) {
  if (!symbol) {
    throw new Error("A symbol is required.");
  }

  const response = await httpClient(
    `${API_ENDPOINTS.assets}/${encodeURIComponent(symbol)}/forecast/latest`
  );

  if (!response || !Array.isArray(response.forecasts) || response.forecasts.length === 0) {
    return [];
  }

  return response.forecasts.map(normalizeForecast).filter(Boolean);
}
