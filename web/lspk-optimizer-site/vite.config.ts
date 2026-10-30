import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

/**
 * Configuração oficial do empacotador Vite para a Landing Page do LSPK Optimizer.
 * Utiliza o plugin oficial do React para compilação Fast Refresh e geração de bundles estáticos.
 */
export default defineConfig({
  plugins: [react()],
  base: './',
});
