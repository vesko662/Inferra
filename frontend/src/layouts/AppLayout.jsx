import { useEffect, useRef, useState } from "react";
import { Link, NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const navItems = [
  { to: "/", label: "Overview" },
  { to: "/coins", label: "Markets" },
  { to: "/about", label: "About" }
];

function getInitials(name) {
  if (!name) return "?";
  const parts = name.trim().split(/\s+/);
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

function UserMenu({ displayName, logout }) {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);

  useEffect(() => {
    function handleClick(e) {
      if (ref.current && !ref.current.contains(e.target)) setOpen(false);
    }
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  return (
    <div className="user-menu" ref={ref}>
      <button
        className="user-menu__trigger"
        type="button"
        onClick={() => setOpen((o) => !o)}
        aria-expanded={open}
        aria-haspopup="true"
      >
        <span className="user-menu__avatar">{getInitials(displayName)}</span>
      </button>
      {open && (
        <div className="user-menu__dropdown">
          <div className="user-menu__info">
            <span className="user-menu__label">Signed in as</span>
            <strong className="user-menu__name">{displayName}</strong>
          </div>
          <div className="user-menu__divider" />
          <button
            className="user-menu__action"
            type="button"
            onClick={() => { setOpen(false); logout(); }}
          >
            Logout
          </button>
        </div>
      )}
    </div>
  );
}

function AppLayout() {
  const { error, isAdmin, isAuthenticated, login, logout, register, user } = useAuth();
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
            {isAdmin && (
              <NavLink
                to="/admin"
                className={({ isActive }) =>
                  isActive ? "main-nav__link main-nav__link--active" : "main-nav__link"
                }
              >
                Admin
              </NavLink>
            )}
          </nav>

          <div className="auth-bar">
            {error ? <span className="auth-bar__error">{error}</span> : null}

            {isAuthenticated ? (
              <UserMenu displayName={displayName} logout={logout} />
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
