import { createContext, useContext, useEffect, useMemo, useReducer } from 'react';
import { useNavigate } from 'react-router-dom';
import { clearStoredAuth, getStoredAuth, setSessionExpiredHandler, storeAuth } from '../services/api.js';
import * as authService from '../services/authService.js';
import { isAdminRole } from '../utils/roles.js';

const AuthContext = createContext(null);

const initialAuth = getStoredAuth();

function authReducer(state, action) {
  switch (action.type) {
    case 'START':
      return { ...state, loading: true, error: null };
    case 'SUCCESS':
      return { user: action.payload.usuario ?? action.payload, token: action.payload.token ?? state.token, expiresAt: action.payload.expiraEn ?? state.expiresAt, loading: false, error: null };
    case 'ERROR':
      return { ...state, loading: false, error: action.payload };
    case 'LOGOUT':
      return { user: null, token: null, expiresAt: null, loading: false, error: null };
    default:
      return state;
  }
}

export function AuthProvider({ children }) {
  const navigate = useNavigate();
  const [state, dispatch] = useReducer(authReducer, {
    user: initialAuth?.usuario ?? initialAuth?.user ?? null,
    token: initialAuth?.token ?? null,
    expiresAt: initialAuth?.expiraEn ?? initialAuth?.expiresAt ?? null,
    loading: false,
    error: null
  });

  useEffect(() => {
    setSessionExpiredHandler(() => {
      dispatch({ type: 'LOGOUT' });
      navigate('/login?session=expired', { replace: true });
    });
    return () => setSessionExpiredHandler(null);
  }, [navigate]);

  const value = useMemo(() => ({
    ...state,
    isAuthenticated: Boolean(state.token && state.user),
    isAdmin: isAdminRole(state.user?.rol),
    async login(credentials) {
      dispatch({ type: 'START' });
      const auth = await authService.login(credentials);
      storeAuth(auth);
      dispatch({ type: 'SUCCESS', payload: auth });
      return auth;
    },
    async register(payload) {
      dispatch({ type: 'START' });
      const auth = await authService.register(payload);
      storeAuth(auth);
      dispatch({ type: 'SUCCESS', payload: auth });
      return auth;
    },
    async refreshProfile() {
      dispatch({ type: 'START' });
      const usuario = await authService.getMe();
      const auth = { token: state.token, expiraEn: state.expiresAt, usuario };
      storeAuth(auth);
      dispatch({ type: 'SUCCESS', payload: auth });
      return usuario;
    },
    logout() {
      clearStoredAuth();
      dispatch({ type: 'LOGOUT' });
      navigate('/login');
    }
  }), [navigate, state]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth debe usarse dentro de AuthProvider');
  return context;
}
