import { Link } from "react-router-dom";

function NotFoundPage() {
  return (
    <div className="container page-section">
      <div className="surface-card empty-state">
        <h1>Page not found</h1>
        <p>The page you requested does not exist or may have moved.</p>
        <Link className="button button--primary" to="/">
          Return to dashboard
        </Link>
      </div>
    </div>
  );
}

export default NotFoundPage;
