import { createContext, useContext, useEffect, useMemo, useState } from "react";
import LoadingState from "../components/common/LoadingState";
import keycloak, {
  getParsedUser,
  initKeycloak,
  login as keycloakLogin,
  logout as keycloakLogout,
  refreshToken,
  register as keycloakRegister
} from "./keycloak";

const AuthContext = createContext(null);

function AuthProvider({ children }) {
  const [isInitialized, setIsInitialized] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [token, setToken] = useState("");
  const [user, setUser] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    let isMounted = true;

    async function bootstrapAuth() {
      try {
        const authenticated = await initKeycloak();

        if (!isMounted) {
          return;
        }

        setIsAuthenticated(authenticated);
        setToken(keycloak.token || "");
        setUser(getParsedUser());
      } catch (authError) {
        if (!isMounted) {
          return;
        }

        setError(authError.message || "Unable to initialize authentication.");
      } finally {
        if (isMounted) {
          setIsInitialized(true);
        }
      }
    }

    bootstrapAuth();

    keycloak.onAuthSuccess = async () => {
      setIsAuthenticated(true);
      setUser(getParsedUser());
      setToken(await refreshToken(30));
    };

    keycloak.onAuthLogout = () => {
      setIsAuthenticated(false);
      setUser(null);
      setToken("");
    };

    keycloak.onTokenExpired = async () => {
      try {
        const nextToken = await refreshToken(30);
        setToken(nextToken);
        setUser(getParsedUser());
      } catch {
        setIsAuthenticated(false);
        setToken("");
      }
    };

    return () => {
      isMounted = false;
      keycloak.onAuthSuccess = undefined;
      keycloak.onAuthLogout = undefined;
      keycloak.onTokenExpired = undefined;
    };
  }, []);

  useEffect(() => {
    if (!isInitialized) {
      return undefined;
    }

    const intervalId = window.setInterval(async () => {
      if (!keycloak.authenticated) {
        return;
      }

      try {
        const nextToken = await refreshToken(60);
        setToken(nextToken);
        setUser(getParsedUser());
      } catch {
        setIsAuthenticated(false);
        setToken("");
      }
    }, 60 * 1000);

    return () => {
      window.clearInterval(intervalId);
    };
  }, [isInitialized]);

  const value = useMemo(
    () => ({
      isInitialized,
      isAuthenticated,
      user,
      token,
      error,
      login: keycloakLogin,
      register: keycloakRegister,
      logout: keycloakLogout,
      getAccessToken: async () => {
        const nextToken = await refreshToken(30);
        setToken(nextToken);
        return nextToken;
      }
    }),
    [error, isAuthenticated, isInitialized, token, user]
  );

  if (!isInitialized) {
    return (
      <div className="container page-section">
        <LoadingState label="Initializing authentication..." />
      </div>
    );
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used within AuthProvider.");
  }

  return context;
}

export default AuthProvider;
