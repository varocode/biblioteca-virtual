import { Navigate, Route, Routes } from 'react-router-dom';
import Layout from './components/Layout.jsx';
import ProtectedRoute from './components/ProtectedRoute.jsx';
import CatalogPage from './pages/CatalogPage.jsx';
import BookDetailPage from './pages/BookDetailPage.jsx';
import LoginPage from './pages/LoginPage.jsx';
import RegisterPage from './pages/RegisterPage.jsx';
import ProfilePage from './pages/ProfilePage.jsx';
import ReaderDashboardPage from './pages/ReaderDashboardPage.jsx';
import ReaderLoansPage from './pages/ReaderLoansPage.jsx';
import ReaderReservationsPage from './pages/ReaderReservationsPage.jsx';
import ReaderFinesPage from './pages/ReaderFinesPage.jsx';
import ReaderNotificationsPage from './pages/ReaderNotificationsPage.jsx';
import AdminDashboardPage from './pages/AdminDashboardPage.jsx';
import AdminUsersPage from './pages/AdminUsersPage.jsx';
import AdminCatalogPage from './pages/AdminCatalogPage.jsx';
import AdminCirculationPage from './pages/AdminCirculationPage.jsx';
import AdminInventoryPage from './pages/AdminInventoryPage.jsx';
import AdminAuditPage from './pages/AdminAuditPage.jsx';
import NotFoundPage from './pages/NotFoundPage.jsx';

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<CatalogPage />} />
        <Route path="catalogo" element={<CatalogPage />} />
        <Route path="catalogo/:id" element={<BookDetailPage />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="registro" element={<RegisterPage />} />
        <Route element={<ProtectedRoute />}>
          <Route path="perfil" element={<ProfilePage />} />
          <Route path="mi-biblioteca" element={<ReaderDashboardPage />} />
          <Route path="prestamos" element={<ReaderLoansPage />} />
          <Route path="reservas" element={<ReaderReservationsPage />} />
          <Route path="multas" element={<ReaderFinesPage />} />
          <Route path="notificaciones" element={<ReaderNotificationsPage />} />
        </Route>
        <Route element={<ProtectedRoute roles={["Administrador"]} />}>
          <Route path="admin" element={<AdminDashboardPage />} />
          <Route path="admin/usuarios" element={<AdminUsersPage />} />
          <Route path="admin/catalogo" element={<AdminCatalogPage />} />
          <Route path="admin/circulacion" element={<AdminCirculationPage />} />
          <Route path="admin/inventario" element={<AdminInventoryPage />} />
          <Route path="admin/auditoria" element={<AdminAuditPage />} />
        </Route>
        <Route path="/libros" element={<Navigate to="/catalogo" replace />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
