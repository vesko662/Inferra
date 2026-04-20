import CoinListSection from "../sections/CoinListSection";

function CoinsPage() {
  return (
    <div className="page">
      <section className="container page-heading">
        <div>
          <div className="eyebrow">Market directory</div>
          <h1>Tracked crypto assets</h1>
          <p>
            Browse assets returned by the backend, search across visible values, and open any item
            with a routed `symbol` for detail and snapshot data.
          </p>
        </div>
      </section>

      <section className="container page-section">
        <CoinListSection
          title="All tracked assets"
          description="A CoinGecko-style market view with compact rows, fresh snapshot metrics, and quick navigation into each dedicated asset page."
          pageSize={10}
          enablePagination
        />
      </section>
    </div>
  );
}

export default CoinsPage;
