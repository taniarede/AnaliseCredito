import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// O frontend corre em http://localhost:5173.
// Os pedidos a /api são reencaminhados para a API .NET (http://localhost:5080),
// por isso o browser nunca fala diretamente com outra porta.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
});
