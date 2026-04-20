import Keycloak from "keycloak-js";

const keycloakConfig = {
  url: import.meta.env.VITE_KEYCLOAK_URL,
  realm: import.meta.env.VITE_KEYCLOAK_REALM,
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID
};

if (!keycloakConfig.url || !keycloakConfig.realm || !keycloakConfig.clientId) {
  throw new Error("Missing Keycloak environment variables.");
}

const keycloak = new Keycloak(keycloakConfig);
let initPromise = null;

export async function initKeycloak() {
  if (!initPromise) {
    initPromise = keycloak.init({
      onLoad: "check-sso",
      pkceMethod: "S256",
      checkLoginIframe: false
    });
  }

  return initPromise;
}

export function login() {
  return keycloak.login({
    redirectUri: window.location.href
  });
}

export function register() {
  return keycloak.register({
    redirectUri: window.location.href
  });
}

export function logout() {
  return keycloak.logout({
    redirectUri: window.location.origin
  });
}

export async function refreshToken(minValidity = 60) {
  if (!keycloak.authenticated) {
    return "";
  }

  await keycloak.updateToken(minValidity);
  return keycloak.token || "";
}

export async function getAccessToken(minValidity = 60) {
  if (!keycloak.authenticated) {
    return "";
  }

  return refreshToken(minValidity);
}

export function getParsedUser() {
  const parsedToken = keycloak.tokenParsed;

  if (!parsedToken) {
    return null;
  }

  return {
    id: parsedToken.sub || "",
    username: parsedToken.preferred_username || "",
    email: parsedToken.email || "",
    firstName: parsedToken.given_name || "",
    lastName: parsedToken.family_name || "",
    fullName: parsedToken.name || ""
  };
}

export default keycloak;
