import { Route, Routes } from "react-router-dom";
import AppLayout from "./layouts/AppLayout";
import CoinDetailPage from "./pages/CoinDetailPage";
import CoinsPage from "./pages/CoinsPage";
import DashboardPage from "./pages/DashboardPage";
import NotFoundPage from "./pages/NotFoundPage";

function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<DashboardPage />} />
        <Route path="coins" element={<CoinsPage />} />
        <Route path="coins/:symbol" element={<CoinDetailPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}

export default App;
