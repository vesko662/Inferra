import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const navItems = [
  { to: "/", label: "Dashboard" },
  { to: "/coins", label: "Markets" }
];

function AppLayout() {
  const { error, isAuthenticated, login, logout, register, user } = useAuth();
  const displayName = user?.fullName || user?.username || user?.email || "Authenticated user";

  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="app-header__inner container">
          <NavLink to="/" className="brand" aria-label="Inferra home">
            <span className="brand__mark">I</span>
            <span>
              <strong>Inferra</strong>
              <small>Crypto intelligence, simplified</small>
            </span>
          </NavLink>

          <nav className="main-nav" aria-label="Primary navigation">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  isActive ? "main-nav__link main-nav__link--active" : "main-nav__link"
                }
              >
                {item.label}
              </NavLink>
            ))}
          </nav>

          <div className="auth-bar">
            {error ? <span className="auth-bar__error">{error}</span> : null}

            {isAuthenticated ? (
              <>
                <div className="auth-bar__user">
                  <span className="auth-bar__label">Signed in</span>
                  <strong>{displayName}</strong>
                </div>
                <button className="button button--ghost" type="button" onClick={logout}>
                  Logout
                </button>
              </>
            ) : (
              <>
                <button className="button button--ghost" type="button" onClick={register}>
                  Register
                </button>
                <button className="button button--primary" type="button" onClick={login}>
                  Login
                </button>
              </>
            )}
          </div>
        </div>
      </header>

      <main className="app-main">
        <Outlet />
      </main>
    </div>
  );
}

export default AppLayout;
