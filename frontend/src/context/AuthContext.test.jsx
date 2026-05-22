import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, useLocation } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { AuthProvider } from './AuthContext.jsx';
import { api } from '../services/api.js';

function LocationProbe() {
  const location = useLocation();
  return <p data-testid="location">{location.pathname}{location.search}</p>;
}

function TriggerExpiredSession() {
  api.get('/auth/me').catch(() => {});
  return <LocationProbe />;
}

describe('AuthProvider', () => {
  it('redirects to login when an API request returns 401', async () => {
    api.defaults.adapter = () => Promise.reject({ response: { status: 401 } });

    render(
      <MemoryRouter initialEntries={["/perfil"]}>
        <AuthProvider>
          <TriggerExpiredSession />
        </AuthProvider>
      </MemoryRouter>
    );

    await waitFor(() => expect(screen.getByTestId('location')).toHaveTextContent('/login?session=expired'));
    api.defaults.adapter = undefined;
  });
});
