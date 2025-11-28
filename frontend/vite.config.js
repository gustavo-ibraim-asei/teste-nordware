import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'https://localhost:60545',
        changeOrigin: true,
        secure: false // Permitir certificados auto-assinados
      },
      '/orderHub': {
        target: 'https://localhost:60545',
        changeOrigin: true,
        secure: false, // Permitir certificados auto-assinados
        ws: true, // Habilitar WebSocket
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('Proxy error:', err);
          });
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('Proxying request:', req.method, req.url);
          });
        }
      }
    }
  }
})





