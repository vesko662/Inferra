const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL;

export const API_CONFIG = {
  baseUrl: configuredBaseUrl || "",
  timeoutMs: 15000
};

export const API_ENDPOINTS = {
  assets: "/api/assets"
};
