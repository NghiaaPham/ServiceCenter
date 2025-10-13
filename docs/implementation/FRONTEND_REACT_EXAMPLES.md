# 🚀 REACT FRONTEND IMPLEMENTATION EXAMPLES

## 📦 Required Packages

```bash
npm install axios react-router-dom @tanstack/react-query
npm install -D @types/node
```

---

## 1. API CLIENT SETUP

### `src/services/api.js`

```javascript
import axios from 'axios';

// Base API URL
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5153/api';

// Create axios instance
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - Thêm token vào header
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor - Xử lý lỗi
api.interceptors.response.use(
  (response) => response.data,
  (error) => {
    const { status } = error.response || {};

    if (status === 401) {
      // Token expired - Redirect to login
      localStorage.removeItem('accessToken');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }

    return Promise.reject(error.response?.data || error);
  }
);

export default api;
```

---

## 2. AUTHENTICATION SERVICE

### `src/services/authService.js`

```javascript
import api from './api';

export const authService = {
  // Đăng ký
  async register(data) {
    return api.post('/customer-registration/register', data);
  },

  // Xác thực email
  async verifyEmail(email, token) {
    return api.post('/verification/verify-email', { email, token });
  },

  // Đăng nhập
  async login(email, password) {
    const response = await api.post('/auth/login', { email, password });

    // Lưu token và user info
    if (response.data.token) {
      localStorage.setItem('accessToken', response.data.token);
      localStorage.setItem('refreshToken', response.data.refreshToken);
      localStorage.setItem('user', JSON.stringify(response.data.user));
    }

    return response;
  },

  // Đăng xuất
  async logout() {
    await api.post('/auth/logout');
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  },

  // Lấy user hiện tại
  getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  // Check đã đăng nhập chưa
  isAuthenticated() {
    return !!localStorage.getItem('accessToken');
  },

  // Đổi mật khẩu
  async changePassword(oldPassword, newPassword) {
    return api.put('/auth/change-password', { oldPassword, newPassword });
  }
};
```

---

## 3. APPOINTMENT SERVICE

### `src/services/appointmentService.js`

```javascript
import api from './api';

export const appointmentService = {
  // Xem danh sách appointment của tôi
  async getMyAppointments(params = {}) {
    return api.get('/appointments/my-appointments', { params });
  },

  // Xem chi tiết appointment
  async getAppointmentById(id) {
    return api.get(`/appointments/${id}`);
  },

  // Tạo appointment mới (Smart Subscription)
  async createAppointment(data) {
    return api.post('/appointments', data);
  },

  // Hủy appointment
  async cancelAppointment(id, reason) {
    return api.post(`/appointments/${id}/cancel`, { reason });
  },

  // Reschedule appointment
  async rescheduleAppointment(id, data) {
    return api.post(`/appointments/${id}/reschedule`, data);
  },

  // Xem appointment sắp tới
  async getUpcomingAppointments(limit = 5) {
    return api.get('/appointments/my-appointments/upcoming', {
      params: { limit }
    });
  }
};
```

---

## 4. SUBSCRIPTION SERVICE

### `src/services/subscriptionService.js`

```javascript
import api from './api';

export const subscriptionService = {
  // Xem subscriptions của tôi
  async getMySubscriptions(statusFilter) {
    return api.get('/package-subscriptions/my-subscriptions', {
      params: { statusFilter }
    });
  },

  // Xem chi tiết subscription
  async getSubscriptionById(id) {
    return api.get(`/package-subscriptions/${id}`);
  },

  // Xem usage history
  async getSubscriptionUsage(id) {
    return api.get(`/package-subscriptions/${id}/usage`);
  },

  // Xem active subscriptions cho vehicle
  async getActiveSubscriptionsByVehicle(vehicleId) {
    return api.get(`/package-subscriptions/vehicle/${vehicleId}/active`);
  },

  // Mua package mới
  async purchasePackage(data) {
    return api.post('/package-subscriptions/purchase', data);
  },

  // Hủy subscription
  async cancelSubscription(id, reason) {
    return api.post(`/package-subscriptions/${id}/cancel`, { reason });
  }
};
```

---

## 5. REACT HOOKS

### `src/hooks/useAuth.js`

```javascript
import { useState, useEffect, createContext, useContext } from 'react';
import { authService } from '../services/authService';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Check if user is logged in
    const currentUser = authService.getCurrentUser();
    setUser(currentUser);
    setLoading(false);
  }, []);

  const login = async (email, password) => {
    const response = await authService.login(email, password);
    setUser(response.data.user);
    return response;
  };

  const logout = async () => {
    await authService.logout();
    setUser(null);
  };

  const register = async (data) => {
    return authService.register(data);
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, register }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
```

---

### `src/hooks/useAppointments.js`

```javascript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { appointmentService } from '../services/appointmentService';

export const useAppointments = () => {
  const queryClient = useQueryClient();

  // Lấy danh sách appointments
  const { data: appointments, isLoading, error } = useQuery({
    queryKey: ['appointments'],
    queryFn: () => appointmentService.getMyAppointments(),
  });

  // Tạo appointment mới
  const createMutation = useMutation({
    mutationFn: appointmentService.createAppointment,
    onSuccess: (data) => {
      queryClient.invalidateQueries(['appointments']);
      return data;
    },
  });

  // Hủy appointment
  const cancelMutation = useMutation({
    mutationFn: ({ id, reason }) => appointmentService.cancelAppointment(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries(['appointments']);
    },
  });

  return {
    appointments: appointments?.data || [],
    isLoading,
    error,
    createAppointment: createMutation.mutate,
    cancelAppointment: cancelMutation.mutate,
    isCreating: createMutation.isPending,
    isCancelling: cancelMutation.isPending,
  };
};
```

---

### `src/hooks/useSubscriptions.js`

```javascript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { subscriptionService } from '../services/subscriptionService';

export const useSubscriptions = () => {
  const queryClient = useQueryClient();

  // Lấy subscriptions
  const { data: subscriptions, isLoading, error } = useQuery({
    queryKey: ['subscriptions'],
    queryFn: () => subscriptionService.getMySubscriptions(),
  });

  // Mua package
  const purchaseMutation = useMutation({
    mutationFn: subscriptionService.purchasePackage,
    onSuccess: () => {
      queryClient.invalidateQueries(['subscriptions']);
    },
  });

  return {
    subscriptions: subscriptions?.data || [],
    isLoading,
    error,
    purchasePackage: purchaseMutation.mutate,
    isPurchasing: purchaseMutation.isPending,
  };
};
```

---

## 6. REACT COMPONENTS

### `src/pages/LoginPage.jsx`

```jsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await login(email, password);
      navigate('/dashboard');
    } catch (err) {
      setError(err.message || 'Đăng nhập thất bại');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <h1>Đăng nhập</h1>

      {error && <div className="error-message">{error}</div>}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label>Email</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>

        <div className="form-group">
          <label>Mật khẩu</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        <button type="submit" disabled={loading}>
          {loading ? 'Đang đăng nhập...' : 'Đăng nhập'}
        </button>
      </form>

      <p>
        Chưa có tài khoản? <a href="/register">Đăng ký ngay</a>
      </p>
    </div>
  );
}
```

---

### `src/pages/BookingPage.jsx`

```jsx
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { appointmentService } from '../services/appointmentService';
import { subscriptionService } from '../services/subscriptionService';
import { toast } from 'react-toastify';

export default function BookingPage() {
  const [selectedVehicle, setSelectedVehicle] = useState(null);
  const [selectedService, setSelectedService] = useState(null);
  const [selectedDate, setSelectedDate] = useState('');
  const [selectedSlot, setSelectedSlot] = useState(null);
  const [activeSubscription, setActiveSubscription] = useState(null);
  const [loading, setLoading] = useState(false);

  const navigate = useNavigate();

  // Load active subscription khi chọn vehicle
  useEffect(() => {
    if (selectedVehicle && selectedService) {
      loadActiveSubscription();
    }
  }, [selectedVehicle, selectedService]);

  const loadActiveSubscription = async () => {
    try {
      const response = await subscriptionService.getActiveSubscriptionsByVehicle(
        selectedVehicle.vehicleId
      );

      // Check if service có trong subscription không
      const subscription = response.data.find(sub =>
        sub.packageServices.some(ps => ps.serviceId === selectedService.serviceId)
      );

      setActiveSubscription(subscription);
    } catch (err) {
      console.error('Error loading subscription:', err);
    }
  };

  const handleBooking = async () => {
    setLoading(true);

    try {
      const bookingData = {
        serviceCenterId: 1,
        vehicleId: selectedVehicle.vehicleId,
        appointmentDate: selectedDate,
        slotId: selectedSlot.slotId,
        services: [
          {
            serviceId: selectedService.serviceId,
            quantity: 1,
            notes: ''
          }
        ],
        notes: ''
      };

      const response = await appointmentService.createAppointment(bookingData);

      // Kiểm tra subscription có được apply không
      if (response.data.subscriptionApplied) {
        toast.success(
          `Đặt lịch thành công! Dịch vụ được áp dụng từ ${response.data.subscriptionDetails.packageName}. ` +
          `Còn lại ${response.data.subscriptionDetails.remainingServicesAfter} dịch vụ.`
        );
      } else {
        toast.success(
          `Đặt lịch thành công! Vui lòng thanh toán ${response.data.totalAmount.toLocaleString()} VNĐ khi đến.`
        );
      }

      navigate(`/appointments/${response.data.appointmentId}`);
    } catch (err) {
      toast.error(err.message || 'Đặt lịch thất bại');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="booking-page">
      <h1>Đặt lịch bảo dưỡng</h1>

      {/* Vehicle Selection */}
      <div className="section">
        <h2>1. Chọn xe</h2>
        {/* Vehicle selection UI */}
      </div>

      {/* Service Selection */}
      <div className="section">
        <h2>2. Chọn dịch vụ</h2>
        {/* Service selection UI */}
      </div>

      {/* Subscription Info */}
      {activeSubscription && (
        <div className="subscription-info success">
          <h3>✅ Bạn có gói đăng ký áp dụng được!</h3>
          <p>
            Gói: <strong>{activeSubscription.packageName}</strong><br />
            Còn lại: <strong>{activeSubscription.remainingServices}</strong> dịch vụ<br />
            <span className="highlight">Bạn không cần thanh toán cho dịch vụ này!</span>
          </p>
        </div>
      )}

      {!activeSubscription && selectedService && (
        <div className="subscription-info warning">
          <p>
            Bạn chưa có gói đăng ký cho dịch vụ này.<br />
            Giá: <strong>{selectedService.price.toLocaleString()} VNĐ</strong>
          </p>
          <button onClick={() => navigate('/packages')}>
            Xem các gói đăng ký
          </button>
        </div>
      )}

      {/* Date & Time Selection */}
      <div className="section">
        <h2>3. Chọn ngày và giờ</h2>
        {/* Date picker and time slot selection */}
      </div>

      {/* Booking Button */}
      <button
        className="btn-primary btn-large"
        onClick={handleBooking}
        disabled={loading || !selectedVehicle || !selectedService || !selectedSlot}
      >
        {loading ? 'Đang đặt lịch...' : 'Xác nhận đặt lịch'}
      </button>
    </div>
  );
}
```

---

### `src/pages/MyAppointmentsPage.jsx`

```jsx
import { useAppointments } from '../hooks/useAppointments';
import { formatDate } from '../utils/dateUtils';

export default function MyAppointmentsPage() {
  const { appointments, isLoading, cancelAppointment, isCancelling } = useAppointments();

  if (isLoading) {
    return <div>Đang tải...</div>;
  }

  const handleCancel = (appointmentId) => {
    if (window.confirm('Bạn có chắc muốn hủy lịch hẹn này?')) {
      cancelAppointment({
        id: appointmentId,
        reason: 'Khách hàng yêu cầu hủy'
      });
    }
  };

  return (
    <div className="appointments-page">
      <h1>Lịch hẹn của tôi</h1>

      {appointments.length === 0 ? (
        <div className="empty-state">
          <p>Bạn chưa có lịch hẹn nào</p>
          <button onClick={() => navigate('/booking')}>
            Đặt lịch ngay
          </button>
        </div>
      ) : (
        <div className="appointments-list">
          {appointments.map((apt) => (
            <div key={apt.appointmentId} className="appointment-card">
              <div className="appointment-header">
                <h3>{apt.appointmentCode}</h3>
                <span className={`status ${apt.status.toLowerCase()}`}>
                  {apt.status}
                </span>
              </div>

              <div className="appointment-body">
                <p><strong>Ngày:</strong> {formatDate(apt.appointmentDate)}</p>
                <p><strong>Giờ:</strong> {apt.slotTime}</p>
                <p><strong>Xe:</strong> {apt.vehicle.licensePlate}</p>
                <p><strong>Dịch vụ:</strong></p>
                <ul>
                  {apt.services.map((service, idx) => (
                    <li key={idx}>
                      {service.serviceName}
                      {service.serviceSource === 'Subscription' && (
                        <span className="badge badge-success">
                          Từ gói đăng ký
                        </span>
                      )}
                    </li>
                  ))}
                </ul>

                {apt.subscriptionUsed ? (
                  <div className="price-info success">
                    ✅ Đã áp dụng gói đăng ký - Không cần thanh toán
                  </div>
                ) : (
                  <div className="price-info">
                    Tổng tiền: <strong>{apt.totalAmount.toLocaleString()} VNĐ</strong>
                  </div>
                )}
              </div>

              <div className="appointment-actions">
                <button onClick={() => navigate(`/appointments/${apt.appointmentId}`)}>
                  Chi tiết
                </button>
                {apt.status === 'Pending' && (
                  <button
                    className="btn-danger"
                    onClick={() => handleCancel(apt.appointmentId)}
                    disabled={isCancelling}
                  >
                    Hủy lịch
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
```

---

### `src/pages/MySubscriptionsPage.jsx`

```jsx
import { useSubscriptions } from '../hooks/useSubscriptions';

export default function MySubscriptionsPage() {
  const { subscriptions, isLoading } = useSubscriptions();

  if (isLoading) {
    return <div>Đang tải...</div>;
  }

  return (
    <div className="subscriptions-page">
      <h1>Gói đăng ký của tôi</h1>

      {subscriptions.length === 0 ? (
        <div className="empty-state">
          <p>Bạn chưa có gói đăng ký nào</p>
          <button onClick={() => navigate('/packages')}>
            Xem các gói đăng ký
          </button>
        </div>
      ) : (
        <div className="subscriptions-list">
          {subscriptions.map((sub) => (
            <div key={sub.subscriptionId} className="subscription-card">
              <div className="card-header">
                <h3>{sub.packageName}</h3>
                <span className={`status ${sub.status.toLowerCase()}`}>
                  {sub.status}
                </span>
              </div>

              <div className="card-body">
                <p><strong>Xe:</strong> {sub.vehiclePlate}</p>
                <p><strong>Thời hạn:</strong> {sub.startDate} - {sub.endDate}</p>

                <div className="usage-bar">
                  <div className="usage-info">
                    <span>Đã dùng: {sub.usedServices}/{sub.totalServices}</span>
                    <span>Còn lại: {sub.remainingServices}</span>
                  </div>
                  <div className="progress-bar">
                    <div
                      className="progress"
                      style={{
                        width: `${(sub.usedServices / sub.totalServices) * 100}%`
                      }}
                    />
                  </div>
                </div>

                <div className="price-info">
                  Giá trị gói: <strong>{sub.purchasePrice.toLocaleString()} VNĐ</strong>
                  {sub.discountPercent > 0 && (
                    <span className="discount">(-{sub.discountPercent}%)</span>
                  )}
                </div>
              </div>

              <div className="card-actions">
                <button onClick={() => navigate(`/subscriptions/${sub.subscriptionId}`)}>
                  Xem chi tiết
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
```

---

## 7. PROTECTED ROUTE

### `src/components/ProtectedRoute.jsx`

```jsx
import { Navigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export default function ProtectedRoute({ children }) {
  const { user, loading } = useAuth();

  if (loading) {
    return <div>Loading...</div>;
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  return children;
}
```

---

## 8. APP ROUTER

### `src/App.jsx`

```jsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from './hooks/useAuth';
import ProtectedRoute from './components/ProtectedRoute';

import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashboardPage';
import BookingPage from './pages/BookingPage';
import MyAppointmentsPage from './pages/MyAppointmentsPage';
import MySubscriptionsPage from './pages/MySubscriptionsPage';
import PackagesPage from './pages/PackagesPage';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            {/* Public Routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Protected Routes */}
            <Route
              path="/dashboard"
              element={
                <ProtectedRoute>
                  <DashboardPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/booking"
              element={
                <ProtectedRoute>
                  <BookingPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/appointments"
              element={
                <ProtectedRoute>
                  <MyAppointmentsPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/subscriptions"
              element={
                <ProtectedRoute>
                  <MySubscriptionsPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/packages"
              element={
                <ProtectedRoute>
                  <PackagesPage />
                </ProtectedRoute>
              }
            />

            {/* Default Route */}
            <Route path="/" element={<Navigate to="/dashboard" />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
```

---

## 9. UTILITY FUNCTIONS

### `src/utils/dateUtils.js`

```javascript
export function formatDate(dateString) {
  const date = new Date(dateString);
  return date.toLocaleDateString('vi-VN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  });
}

export function formatDateTime(dateString) {
  const date = new Date(dateString);
  return date.toLocaleString('vi-VN');
}
```

---

### `src/utils/currencyUtils.js`

```javascript
export function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(amount);
}
```

---

## 10. ENVIRONMENT VARIABLES

### `.env`

```env
REACT_APP_API_URL=http://localhost:5153/api
REACT_APP_ENV=development
```

### `.env.production`

```env
REACT_APP_API_URL=https://api.evservicecenter.com/api
REACT_APP_ENV=production
```

---

## ✅ SUMMARY

Với các examples trên, frontend developer có thể:

1. ✅ Setup API client với Axios
2. ✅ Implement authentication flow
3. ✅ Sử dụng React Hooks cho state management
4. ✅ Integrate với React Query cho data fetching
5. ✅ Implement Smart Subscription Booking UI
6. ✅ Handle errors và loading states
7. ✅ Protect routes với authentication

**Next Steps:**
- Add styling với Tailwind CSS hoặc Material-UI
- Implement form validation với Formik/React Hook Form
- Add toast notifications với react-toastify
- Setup testing với Jest & React Testing Library

---

**Last Updated**: 2024-10-10
