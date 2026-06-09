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
          title="Crypto Markets"
          description="Live prices and 24h performance for all tracked assets."
          pageSize={10}
          enablePagination
          showRefresh={false}
          showLastUpdated={false}
        />
      </section>
    </div>
  );
}

export default CoinsPage;
