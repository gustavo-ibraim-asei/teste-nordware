// Configuração centralizada da API
// Detecta automaticamente o ambiente e usa a URL apropriada

/**
 * Detecta se deve usar proxy (Nginx/Vite) ou URL direta da API
 * 
 * Cenários:
 * 1. Desenvolvimento (Vite): usa proxy do Vite (/api → https://localhost:60545)
 * 2. Produção com Nginx (Docker): usa proxy do Nginx (/api → http://order-management-api:8080)
 * 3. Produção sem Nginx (build local): usa URL direta (https://localhost:60545/api)
 */
const shouldUseProxy = () => {
  // Cenário 1: Desenvolvimento com Vite dev server
  // Vite tem proxy configurado em vite.config.js
  if (import.meta.env.DEV) {
    return true  // Usa proxy do Vite: /api
  }
  
  // Cenário 2 e 3: Produção (build)
  const hostname = window.location.hostname
  const port = window.location.port
  
  // Se estiver em localhost sem porta ou porta 80, provavelmente tem Nginx
  // (Docker Compose mapeia frontend para porta 80 do container)
  if (hostname === 'localhost' && (port === '' || port === '80')) {
    return true  // Usa proxy do Nginx: /api
  }
  
  // Caso contrário, assume que não tem proxy configurado
  // Exemplos: build local rodando em localhost:3000, ou servidor sem nginx
  return false  // Usa URL direta: https://localhost:60545/api
}

// URL base da API
export const API_BASE_URL = shouldUseProxy()
  ? '/api'  // Proxy: Vite (dev) ou Nginx (prod com Docker)
  : 'https://localhost:60545/api'  // URL direta (prod sem Nginx)

// URL do SignalR Hub
export const API_SIGNALR_URL = shouldUseProxy()
  ? '/orderHub'  // Proxy: Vite (dev) ou Nginx (prod com Docker)
  : 'https://localhost:60545/orderHub'  // URL direta (prod sem Nginx)

