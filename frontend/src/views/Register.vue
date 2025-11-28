<template>
  <div class="register-page">
    <div class="register-container">
      <h2>Criar Conta</h2>
      <form @submit.prevent="handleRegister" class="register-form">
        <div class="form-group">
          <label>Email *</label>
          <input 
            v-model="form.email" 
            type="email" 
            placeholder="seu@email.com" 
            required 
          />
        </div>

        <div class="form-group">
          <label>Nome de Usuário *</label>
          <input 
            v-model="form.userName" 
            type="text" 
            placeholder="nomeusuario" 
            required 
          />
        </div>

        <div class="form-group">
          <label>Senha *</label>
          <input 
            v-model="form.password" 
            type="password" 
            placeholder="Mínimo 6 caracteres" 
            required 
            minlength="6"
          />
        </div>

        <div class="form-group">
          <label>Confirmar Senha *</label>
          <input 
            v-model="form.confirmPassword" 
            type="password" 
            placeholder="Digite a senha novamente" 
            required 
            minlength="6"
          />
        </div>

        <div class="form-group">
          <label>Tenant ID *</label>
          <input 
            v-model="form.tenantId" 
            type="text" 
            placeholder="default" 
            required 
          />
          <small class="help-text">Identificador do tenant (ex: default, empresa1, etc.)</small>
        </div>

        <div v-if="error" class="error-message">
          {{ error }}
        </div>

        <div v-if="success" class="success-message">
          {{ success }}
        </div>

        <div class="form-actions">
          <button type="button" @click="goToLogin" class="btn btn-secondary">
            Voltar para Login
          </button>
          <button type="submit" :disabled="loading" class="btn btn-primary">
            {{ loading ? 'Criando conta...' : 'Criar Conta' }}
          </button>
        </div>

        <div class="login-link">
          <p>Já tem uma conta? <a @click="goToLogin" class="link">Fazer login</a></p>
        </div>
      </form>
    </div>
  </div>
</template>

<script>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import axios from 'axios'
import { API_BASE_URL } from '../config/api'

export default {
  name: 'Register',
  setup() {
    const router = useRouter()
    const authStore = useAuthStore()
    const loading = ref(false)
    const error = ref('')
    const success = ref('')

    const form = ref({
      email: '',
      userName: '',
      password: '',
      confirmPassword: '',
      tenantId: 'default'
    })

    const validateForm = () => {
      if (form.value.password !== form.value.confirmPassword) {
        error.value = 'As senhas não coincidem'
        return false
      }

      if (form.value.password.length < 6) {
        error.value = 'A senha deve ter no mínimo 6 caracteres'
        return false
      }

      if (!form.value.email || !form.value.userName || !form.value.tenantId) {
        error.value = 'Preencha todos os campos obrigatórios'
        return false
      }

      return true
    }

    const handleRegister = async () => {
      error.value = ''
      success.value = ''

      if (!validateForm()) {
        return
      }

      loading.value = true

      try {
        const response = await axios.post(
          `${API_BASE_URL}/auth/register`,
          {
            email: form.value.email,
            userName: form.value.userName,
            password: form.value.password,
            tenantId: form.value.tenantId
          }
        )

        success.value = 'Conta criada com sucesso! Redirecionando para login...'
        
        // Aguardar um pouco antes de redirecionar
        setTimeout(() => {
          router.push('/login')
        }, 2000)
      } catch (err) {
        if (err.response?.data?.message) {
          error.value = err.response.data.message
        } else if (err.response?.data?.errors) {
          // Se houver erros de validação
          const validationErrors = Object.values(err.response.data.errors).flat()
          error.value = validationErrors.join(', ')
        } else {
          error.value = 'Erro ao criar conta. Tente novamente.'
        }
      } finally {
        loading.value = false
      }
    }

    const goToLogin = () => {
      router.push('/login')
    }

    return {
      form,
      loading,
      error,
      success,
      handleRegister,
      goToLogin
    }
  }
}
</script>

<style scoped>
.register-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: calc(100vh - 200px);
  padding: 2rem;
}

.register-container {
  background: white;
  padding: 2.5rem;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 500px;
}

.register-container h2 {
  margin-bottom: 1.5rem;
  color: #2563eb;
  text-align: center;
}

.register-form {
  display: flex;
  flex-direction: column;
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #333;
}

.form-group input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.2s;
}

.form-group input:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.help-text {
  display: block;
  margin-top: 0.25rem;
  font-size: 0.875rem;
  color: #666;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
}

.btn {
  flex: 1;
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: #2563eb;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #1e40af;
}

.btn-secondary {
  background: #6b7280;
  color: white;
}

.btn-secondary:hover {
  background: #4b5563;
}

.error-message {
  margin-top: 1rem;
  padding: 0.75rem;
  background: #fee2e2;
  color: #991b1b;
  border-radius: 6px;
  font-size: 0.875rem;
}

.success-message {
  margin-top: 1rem;
  padding: 0.75rem;
  background: #d1fae5;
  color: #065f46;
  border-radius: 6px;
  font-size: 0.875rem;
}

.login-link {
  margin-top: 1.5rem;
  text-align: center;
  padding-top: 1.5rem;
  border-top: 1px solid #e5e7eb;
}

.login-link p {
  color: #666;
  font-size: 0.875rem;
}

.link {
  color: #2563eb;
  cursor: pointer;
  text-decoration: underline;
}

.link:hover {
  color: #1e40af;
}
</style>

