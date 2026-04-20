import { API_CONFIG } from "../config/api";
import { getAccessToken } from "../auth/keycloak";

async function buildHeaders(customHeaders = {}) {
  const token = await getAccessToken(30);

  return {
    Accept: "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...customHeaders
  };
}

async function parseErrorResponse(response) {
  let errorMessage = `Request failed with status ${response.status}.`;

  try {
    const errorPayload = await response.json();
    if (typeof errorPayload?.message === "string" && errorPayload.message.trim()) {
      errorMessage = errorPayload.message;
    }
  } catch {
    // Keep the fallback message when the error body is not JSON.
  }

  return errorMessage;
}

async function httpClient(path, options = {}) {
  const controller = new AbortController();
  const timeoutId = window.setTimeout(() => controller.abort(), API_CONFIG.timeoutMs);

  try {
    const requestOptions = {
      ...options,
      headers: await buildHeaders(options.headers || {}),
      signal: controller.signal
    };

    let response = await fetch(`${API_CONFIG.baseUrl}${path}`, requestOptions);

    if (response.status === 401) {
      response = await fetch(`${API_CONFIG.baseUrl}${path}`, {
        ...options,
        headers: await buildHeaders(options.headers || {}),
        signal: controller.signal
      });
    }

    if (!response.ok) {
      throw new Error(await parseErrorResponse(response));
    }

    const contentType = response.headers.get("content-type") || "";
    if (contentType.includes("application/json")) {
      return response.json();
    }

    return null;
  } catch (error) {
    if (error.name === "AbortError") {
      throw new Error("The request timed out. Please try again.");
    }

    throw new Error(error.message || "The request could not be completed.");
  } finally {
    window.clearTimeout(timeoutId);
  }
}

export default httpClient;
