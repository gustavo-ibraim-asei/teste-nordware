<template>
  <div id="app">
    <nav class="navbar">
      <div class="container">
        <h1>Order Management</h1>
        <div class="nav-actions">
          <span v-if="isAuthenticated" class="user-info">
            {{ username }} ({{ tenantId }})
          </span>
          <button v-if="!isAuthenticated" @click="showLogin = true" class="btn btn-primary">
            Login
          </button>
          <button v-else @click="logout" class="btn btn-secondary">
            Logout
          </button>
        </div>
      </div>
    </nav>

    <LoginModal v-if="showLogin" @close="showLogin = false" @login="handleLogin" />

    <div class="container">
      <router-view v-if="isAuthenticated" />
      <div v-else class="welcome">
        <h2>Bem-vindo ao Order Management</h2>
        <p>Faça login para acessar o dashboard de pedidos</p>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed } from 'vue'
import { useAuthStore } from './stores/auth'
import LoginModal from './components/LoginModal.vue'

export default {
  name: 'App',
  components: {
    LoginModal
  },
  setup() {
    const authStore = useAuthStore()
    const showLogin = ref(false)

    const isAuthenticated = computed(() => authStore.isAuthenticated)
    const username = computed(() => authStore.username)
    const tenantId = computed(() => authStore.tenantId)

    const handleLogin = (loginData) => {
      authStore.login(loginData)
      showLogin.value = false
    }

    const logout = () => {
      authStore.logout()
    }

    return {
      showLogin,
      isAuthenticated,
      username,
      tenantId,
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

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  background-color: #f5f5f5;
}

.navbar {
  background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
  color: white;
  padding: 1rem 0;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.navbar .container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.nav-actions {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.user-info {
  margin-right: 1rem;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.welcome {
  text-align: center;
  padding: 4rem 2rem;
}

.btn {
  padding: 0.5rem 1.5rem;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
  transition: all 0.3s;
}

.btn-primary {
  background-color: white;
  color: #2563eb;
}

.btn-primary:hover {
  background-color: #f0f0f0;
}

.btn-secondary {
  background-color: rgba(255,255,255,0.2);
  color: white;
}

.btn-secondary:hover {
  background-color: rgba(255,255,255,0.3);
}
</style>


