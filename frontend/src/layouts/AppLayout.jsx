import { Link, NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const navItems = [
  { to: "/", label: "Overview" },
  { to: "/coins", label: "Markets" },
  { to: "/about", label: "About" }
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
                end={item.to === "/"}
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

      <footer className="app-footer">
        <div className="app-footer__inner container">
          <Link to="/" className="footer-brand">
            <span className="brand__mark brand__mark--sm">I</span>
            <strong>Inferra</strong>
          </Link>

          <nav className="footer-nav" aria-label="Footer navigation">
            <Link to="/coins" className="footer-nav__link">Markets</Link>
            <Link to="/about" className="footer-nav__link">About</Link>
          </nav>

          <p className="footer-credits">
            Data by <span>Binance</span> &amp; <span>CoinGecko</span>
          </p>
        </div>
      </footer>
    </div>
  );
}

export default AppLayout;
