<template>
  <div class="modal-overlay" @click="$emit('close')">
    <div class="modal" @click.stop>
      <div class="modal-header">
        <div class="modal-icon">🔐</div>
        <h2>Entrar na Conta</h2>
        <button @click="$emit('close')" class="modal-close" aria-label="Fechar">×</button>
      </div>
      <form @submit.prevent="handleSubmit" class="modal-form">
        <div class="form-group">
          <label>Usuário ou Email *</label>
          <div class="input-wrapper">
            <span class="input-icon">👤</span>
            <input 
              v-model="username" 
              type="text" 
              required 
              placeholder="Digite seu usuário ou email"
              class="form-input"
            />
          </div>
        </div>
        <div class="form-group">
          <label>Senha *</label>
          <div class="input-wrapper">
            <span class="input-icon">🔒</span>
            <input 
              v-model="password" 
              type="password" 
              required 
              placeholder="Digite sua senha"
              class="form-input"
            />
          </div>
        </div>
        <div class="form-group">
          <label>Tenant ID <span class="optional">(opcional)</span></label>
          <div class="input-wrapper">
            <span class="input-icon">🏢</span>
            <input 
              v-model="tenantId" 
              type="text" 
              placeholder="default" 
              class="form-input"
            />
          </div>
          <small class="form-hint">Deixe em branco para usar o tenant padrão</small>
        </div>
        <div v-if="error" class="alert alert-error">
          <span>⚠️</span>
          <span>{{ error }}</span>
        </div>
        <div class="form-actions">
          <button 
            type="button" 
            @click="$emit('close')" 
            class="btn btn-outline"
          >
            Cancelar
          </button>
          <button 
            type="submit" 
            class="btn btn-primary"
            :disabled="!username || !password"
          >
            <span>Entrar</span>
            <span>→</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script>
import { ref } from 'vue'

export default {
  name: 'LoginModal',
  emits: ['close', 'login'],
  setup(props, { emit }) {
    const username = ref('')
    const password = ref('')
    const tenantId = ref('')
    const error = ref('')

    const handleSubmit = async () => {
      error.value = ''
      try {
        await emit('login', {
          emailOrUserName: username.value,
          password: password.value,
          tenantId: tenantId.value || null
        })
      } catch (err) {
        error.value = 'Credenciais inválidas. Verifique seu usuário e senha.'
      }
    }

    return {
      username,
      password,
      tenantId,
      error,
      handleSubmit
    }
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(4px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 1rem;
  animation: fadeIn 0.2s;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.modal {
  background: var(--bg-primary);
  padding: 0;
  border-radius: var(--radius-xl);
  width: 100%;
  max-width: 450px;
  box-shadow: var(--shadow-xl);
  animation: slideUp 0.3s;
  overflow: hidden;
}

@keyframes slideUp {
  from {
    transform: translateY(20px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.modal-header {
  background: linear-gradient(135deg, var(--primary-color) 0%, var(--primary-dark) 100%);
  color: white;
  padding: 2rem;
  text-align: center;
  position: relative;
}

.modal-icon {
  font-size: 3rem;
  margin-bottom: 0.5rem;
}

.modal-header h2 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
}

.modal-close {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: rgba(255, 255, 255, 0.2);
  border: none;
  color: white;
  font-size: 1.5rem;
  width: 32px;
  height: 32px;
  border-radius: var(--radius-sm);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  line-height: 1;
}

.modal-close:hover {
  background: rgba(255, 255, 255, 0.3);
  transform: rotate(90deg);
}

.modal-form {
  padding: 2rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: var(--text-primary);
  font-size: 0.9375rem;
}

.optional {
  font-weight: normal;
  color: var(--text-secondary);
  font-size: 0.875rem;
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 0.75rem;
  font-size: 1.125rem;
  color: var(--text-secondary);
  z-index: 1;
}

.form-input {
  width: 100%;
  padding: 0.75rem 0.75rem 0.75rem 2.75rem;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  font-size: 0.9375rem;
  font-family: inherit;
  transition: all 0.2s;
  background: var(--bg-primary);
  color: var(--text-primary);
}

.form-input:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.form-input::placeholder {
  color: var(--text-secondary);
  opacity: 0.6;
}

.form-hint {
  display: block;
  margin-top: 0.25rem;
  font-size: 0.8125rem;
  color: var(--text-secondary);
}

.alert {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.875rem;
  border-radius: var(--radius-md);
  font-size: 0.9375rem;
  margin-bottom: 1rem;
}

.alert-error {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: 0.9375rem;
  font-weight: 500;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  flex: 1;
}

.btn-primary {
  background: linear-gradient(135deg, var(--primary-color) 0%, var(--primary-dark) 100%);
  color: white;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-outline {
  background: var(--bg-primary);
  color: var(--primary-color);
  border: 2px solid var(--primary-color);
}

.btn-outline:hover {
  background: var(--bg-tertiary);
}

/* Responsive */
@media (max-width: 480px) {
  .modal {
    max-width: 100%;
    margin: 0;
  }

  .modal-header {
    padding: 1.5rem;
  }

  .modal-icon {
    font-size: 2.5rem;
  }

  .modal-header h2 {
    font-size: 1.25rem;
  }

  .modal-form {
    padding: 1.5rem;
  }

  .form-actions {
    flex-direction: column;
  }

  .btn {
    width: 100%;
  }
}
</style>
