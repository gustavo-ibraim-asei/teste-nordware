<template>
  <div id="app">
    <!-- Navbar -->
    <nav class="navbar">
      <div class="navbar-container">
        <div class="navbar-brand">
          <button @click="toggleSidebar" class="menu-toggle" aria-label="Toggle menu">
            <span></span>
            <span></span>
            <span></span>
          </button>
          <h1 class="logo">🛍️ OrderStore</h1>
        </div>
        <div class="nav-actions">
          <span v-if="isAuthenticated" class="user-info">
            <span class="user-name">{{ username }}</span>
            <span class="user-tenant">{{ tenantId }}</span>
          </span>
          <template v-if="!isAuthenticated">
            <router-link to="/register" class="btn btn-outline">
              Cadastrar
            </router-link>
            <button @click="showLogin = true" class="btn btn-primary">
              Entrar
            </button>
          </template>
          <button v-else @click="logout" class="btn btn-outline">
            Sair
          </button>
        </div>
      </div>
    </nav>

    <LoginModal v-if="showLogin && !isRegisterPage" @close="showLogin = false" @login="handleLogin" />

    <!-- Main Container -->
    <div class="app-container">
      <!-- Sidebar -->
      <aside :class="['sidebar', { 'sidebar-open': sidebarOpen }]" @click.self="closeSidebarOnMobile">
        <div class="sidebar-content">
          <div class="sidebar-header">
            <h2>Menu</h2>
            <button @click="toggleSidebar" class="close-sidebar" aria-label="Close menu">×</button>
          </div>
          <nav class="sidebar-nav">
            <router-link to="/" class="nav-item" @click="closeSidebarOnMobile">
              <span class="nav-icon">📊</span>
              <span>Dashboard</span>
            </router-link>
            <router-link to="/products" class="nav-item" @click="closeSidebarOnMobile">
              <span class="nav-icon">📦</span>
              <span>Produtos</span>
            </router-link>
            <router-link to="/skus" class="nav-item" @click="closeSidebarOnMobile">
              <span class="nav-icon">🏷️</span>
              <span>SKUs</span>
            </router-link>
            <router-link to="/stocks" class="nav-item" @click="closeSidebarOnMobile">
              <span class="nav-icon">📋</span>
              <span>Estoques</span>
            </router-link>
            <router-link to="/stock-offices" class="nav-item" @click="closeSidebarOnMobile">
              <span class="nav-icon">🏢</span>
              <span>Filiais</span>
            </router-link>
            <router-link to="/colors" class="nav-item" @click="closeSidebarOnMobile">
              <span class="nav-icon">🎨</span>
              <span>Cores</span>
            </router-link>
            <router-link to="/sizes" class="nav-item" @click="closeSidebarOnMobile">
              <span class="nav-icon">📏</span>
              <span>Tamanhos</span>
            </router-link>
            <router-link to="/price-tables" class="nav-item" @click="closeSidebarOnMobile">
              <span class="nav-icon">💰</span>
              <span>Tabelas de Preços</span>
            </router-link>
            <router-link to="/product-prices" class="nav-item" @click="closeSidebarOnMobile">
              <span class="nav-icon">💵</span>
              <span>Preços</span>
            </router-link>
            <router-link to="/customers" class="nav-item" @click="closeSidebarOnMobile">
              <span class="nav-icon">👥</span>
              <span>Clientes</span>
            </router-link>
            <router-link to="/create-order" class="nav-item nav-item-highlight" @click="closeSidebarOnMobile">
              <span class="nav-icon">🛒</span>
              <span>Novo Pedido</span>
            </router-link>
          </nav>
        </div>
        <div class="sidebar-overlay" @click="closeSidebarOnMobile"></div>
      </aside>

      <!-- Main Content -->
      <main class="main-content" :class="{ 'sidebar-open': sidebarOpen }">
        <router-view v-if="isAuthenticated || isRegisterPage" />
        <div v-else class="welcome-screen">
          <div class="welcome-content">
            <div class="welcome-icon">🛍️</div>
            <h2>Bem-vindo ao OrderStore</h2>
            <p>Sua plataforma completa de gestão de pedidos e estoque</p>
            <div class="welcome-actions">
              <button @click="showLogin = true" class="btn btn-primary btn-large">
                Entrar
              </button>
              <router-link to="/register" class="btn btn-outline btn-large">
                Criar Conta
              </router-link>
            </div>
            <div class="welcome-features">
              <div class="feature-item">
                <span class="feature-icon">📦</span>
                <span>Gestão de Estoque</span>
              </div>
              <div class="feature-item">
                <span class="feature-icon">🛒</span>
                <span>Pedidos Online</span>
              </div>
              <div class="feature-item">
                <span class="feature-icon">📊</span>
                <span>Relatórios</span>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from './stores/auth'
import LoginModal from './components/LoginModal.vue'

export default {
  name: 'App',
  components: {
    LoginModal
  },
  setup() {
    const router = useRouter()
    const route = useRoute()
    const authStore = useAuthStore()
    const showLogin = ref(false)
    const sidebarOpen = ref(false)

    const isAuthenticated = computed(() => authStore.isAuthenticated)
    const username = computed(() => authStore.username)
    const tenantId = computed(() => authStore.tenantId)
    const isRegisterPage = computed(() => route.name === 'Register')

    const toggleSidebar = () => {
      sidebarOpen.value = !sidebarOpen.value
    }

    const closeSidebarOnMobile = () => {
      if (window.innerWidth < 1024) {
        sidebarOpen.value = false
      }
    }

    const handleResize = () => {
      if (window.innerWidth >= 1024) {
        sidebarOpen.value = true
      } else {
        sidebarOpen.value = false
      }
    }

    onMounted(() => {
      handleResize()
      window.addEventListener('resize', handleResize)
    })

    onUnmounted(() => {
      window.removeEventListener('resize', handleResize)
    })

    const handleLogin = async (loginData) => {
      try {
        await authStore.login(loginData)
        showLogin.value = false
        if (route.name !== 'Dashboard') {
          router.push('/')
        }
      } catch (error) {
        throw error
      }
    }

    const logout = () => {
      authStore.logout()
      router.push('/')
    }

    return {
      showLogin,
      isAuthenticated,
      username,
      tenantId,
      isRegisterPage,
      sidebarOpen,
      toggleSidebar,
      closeSidebarOnMobile,
      handleLogin,
      logout
    }
  }
}
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

:root {
  --primary-color: #2563eb;
  --primary-dark: #1e40af;
  --secondary-color: #10b981;
  --danger-color: #ef4444;
  --warning-color: #f59e0b;
  --text-primary: #1f2937;
  --text-secondary: #6b7280;
  --bg-primary: #ffffff;
  --bg-secondary: #f9fafb;
  --bg-tertiary: #f3f4f6;
  --border-color: #e5e7eb;
  --shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
  --shadow-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  --radius-sm: 0.375rem;
  --radius-md: 0.5rem;
  --radius-lg: 0.75rem;
  --radius-xl: 1rem;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  background-color: var(--bg-secondary);
  color: var(--text-primary);
  line-height: 1.6;
}

/* Navbar */
.navbar {
  background: linear-gradient(135deg, var(--primary-color) 0%, var(--primary-dark) 100%);
  color: white;
  padding: 1rem 0;
  box-shadow: var(--shadow-md);
  position: sticky;
  top: 0;
  z-index: 1000;
}

.navbar-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}

.navbar-brand {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.menu-toggle {
  display: none;
  flex-direction: column;
  gap: 4px;
  background: rgba(255, 255, 255, 0.2);
  border: none;
  padding: 0.5rem;
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: background 0.2s;
}

.menu-toggle:hover {
  background: rgba(255, 255, 255, 0.3);
}

.menu-toggle span {
  width: 24px;
  height: 2px;
  background: white;
  border-radius: 2px;
  transition: all 0.3s;
}

.logo {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
}

.nav-actions {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.user-info {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  font-size: 0.875rem;
}

.user-name {
  font-weight: 600;
}

.user-tenant {
  font-size: 0.75rem;
  opacity: 0.8;
}

/* Buttons */
.btn {
  padding: 0.625rem 1.25rem;
  border: none;
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: 0.9375rem;
  font-weight: 500;
  transition: all 0.2s;
  text-decoration: none;
  display: inline-block;
  text-align: center;
}

.btn-primary {
  background-color: white;
  color: var(--primary-color);
}

.btn-primary:hover {
  background-color: var(--bg-tertiary);
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
}

.btn-outline {
  background-color: rgba(255, 255, 255, 0.2);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.3);
}

.btn-outline:hover {
  background-color: rgba(255, 255, 255, 0.3);
}

.btn-large {
  padding: 0.875rem 2rem;
  font-size: 1rem;
}

/* App Container */
.app-container {
  display: flex;
  min-height: calc(100vh - 73px);
  max-width: 1400px;
  margin: 0 auto;
}

/* Sidebar */
.sidebar {
  width: 280px;
  background: var(--bg-primary);
  box-shadow: var(--shadow-md);
  position: fixed;
  left: 0;
  top: 73px;
  height: calc(100vh - 73px);
  overflow-y: auto;
  z-index: 999;
  transform: translateX(-100%);
  transition: transform 0.3s ease;
}

.sidebar-open {
  transform: translateX(0);
}

.sidebar-content {
  padding: 1.5rem;
}

.sidebar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--border-color);
}

.sidebar-header h2 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-primary);
}

.close-sidebar {
  display: none;
  background: none;
  border: none;
  font-size: 2rem;
  color: var(--text-secondary);
  cursor: pointer;
  line-height: 1;
  padding: 0;
  width: 32px;
  height: 32px;
}

.sidebar-nav {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.875rem 1rem;
  color: var(--text-primary);
  text-decoration: none;
  border-radius: var(--radius-md);
  transition: all 0.2s;
  font-weight: 500;
}

.nav-item:hover {
  background-color: var(--bg-tertiary);
  transform: translateX(4px);
}

.nav-item.router-link-active {
  background: linear-gradient(135deg, var(--primary-color) 0%, var(--primary-dark) 100%);
  color: white;
  box-shadow: var(--shadow-sm);
}

.nav-item-highlight {
  background: linear-gradient(135deg, var(--secondary-color) 0%, #059669 100%);
  color: white;
  font-weight: 600;
}

.nav-item-highlight:hover {
  transform: translateX(4px) scale(1.02);
}

.nav-icon {
  font-size: 1.25rem;
}

.sidebar-overlay {
  display: none;
  position: fixed;
  top: 73px;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 998;
}

/* Main Content */
.main-content {
  flex: 1;
  padding: 2rem 1.5rem;
  margin-left: 0;
  transition: margin-left 0.3s ease;
  width: 100%;
}

.main-content.sidebar-open {
  margin-left: 280px;
}

/* Welcome Screen */
.welcome-screen {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: calc(100vh - 200px);
  padding: 2rem;
}

.welcome-content {
  text-align: center;
  max-width: 600px;
}

.welcome-icon {
  font-size: 5rem;
  margin-bottom: 1.5rem;
  animation: bounce 2s infinite;
}

@keyframes bounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

.welcome-content h2 {
  font-size: 2.5rem;
  font-weight: 700;
  margin-bottom: 1rem;
  color: var(--text-primary);
}

.welcome-content p {
  font-size: 1.25rem;
  color: var(--text-secondary);
  margin-bottom: 2rem;
}

.welcome-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-bottom: 3rem;
}

.welcome-features {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1.5rem;
  margin-top: 2rem;
}

.feature-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 1.5rem;
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  transition: all 0.2s;
}

.feature-item:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-md);
}

.feature-icon {
  font-size: 2.5rem;
}

/* Responsive Design */
@media (max-width: 1023px) {
  .menu-toggle {
    display: flex;
  }

  .sidebar {
    transform: translateX(-100%);
  }

  .sidebar-open {
    transform: translateX(0);
  }

  .sidebar-overlay {
    display: block;
  }

  .close-sidebar {
    display: block;
  }

  .main-content {
    margin-left: 0 !important;
  }

  .navbar-container {
    padding: 0 1rem;
  }

  .logo {
    font-size: 1.25rem;
  }

  .user-info {
    display: none;
  }

  .welcome-content h2 {
    font-size: 2rem;
  }

  .welcome-actions {
    flex-direction: column;
  }

  .welcome-features {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .main-content {
    padding: 1rem;
  }

  .welcome-content h2 {
    font-size: 1.75rem;
  }

  .welcome-content p {
    font-size: 1rem;
  }

  .nav-actions {
    gap: 0.5rem;
  }

  .btn {
    padding: 0.5rem 1rem;
    font-size: 0.875rem;
  }
}

@media (min-width: 1024px) {
  .sidebar {
    position: relative;
    transform: translateX(0);
    top: 0;
    height: auto;
  }

  .main-content {
    margin-left: 0;
  }

  .main-content.sidebar-open {
    margin-left: 0;
  }
}

/* Scrollbar */
.sidebar::-webkit-scrollbar {
  width: 6px;
}

.sidebar::-webkit-scrollbar-track {
  background: var(--bg-secondary);
}

.sidebar::-webkit-scrollbar-thumb {
  background: var(--border-color);
  border-radius: 3px;
}

.sidebar::-webkit-scrollbar-thumb:hover {
  background: var(--text-secondary);
}
</style>
