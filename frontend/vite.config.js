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
      }
    }
  }
})





