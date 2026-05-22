import { describe, expect, it } from 'vitest';
import { getErrorMessage } from './api.js';

describe('getErrorMessage', () => {
  it('maps forbidden axios errors to a friendly Spanish message', () => {
    expect(getErrorMessage({ response: { status: 403 } })).toBe('No tienes permisos para realizar esta acción.');
  });

  it('maps network failures to a setup-oriented message', () => {
    expect(getErrorMessage({ code: 'ERR_NETWORK', message: 'Network Error' })).toContain('No pudimos conectar con el servidor');
  });
});
