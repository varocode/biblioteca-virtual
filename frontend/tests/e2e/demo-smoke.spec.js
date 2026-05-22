import { expect, test } from '@playwright/test';

const readerEmail = process.env.E2E_READER_EMAIL ?? 'lector1@test.com';
const readerPassword = process.env.E2E_READER_PASSWORD ?? 'Lector123!';
const adminEmail = process.env.E2E_ADMIN_EMAIL ?? 'admin@biblioteca.com';
const adminPassword = process.env.E2E_ADMIN_PASSWORD ?? 'Admin123!';

test('visitor can open the public catalog', async ({ page }) => {
  await page.goto('/catalogo');
  await expect(page.getByRole('heading', { name: /catálogo/i })).toBeVisible();
  await expect(page.getByText(/disponibles|no hay libros/i)).toBeVisible();
});

test('reader can login and see circulation workflows', async ({ page }) => {
  await page.goto('/login');
  await page.getByLabel('Email').fill(readerEmail);
  await page.getByLabel('Contraseña').fill(readerPassword);
  await page.getByRole('button', { name: 'Ingresar' }).click();

  await page.goto('/prestamos');
  await expect(page.getByRole('heading', { name: 'Mis préstamos' })).toBeVisible();
  await page.goto('/reservas');
  await expect(page.getByRole('heading', { name: 'Mis reservas' })).toBeVisible();
});

test('admin can login and open dashboard/catalog CRUD', async ({ page }) => {
  await page.goto('/login');
  await page.getByLabel('Email').fill(adminEmail);
  await page.getByLabel('Contraseña').fill(adminPassword);
  await page.getByRole('button', { name: 'Ingresar' }).click();

  await page.goto('/admin');
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  await page.goto('/admin/catalogo');
  await expect(page.getByRole('heading', { name: 'Catálogo CRUD' })).toBeVisible();
});
