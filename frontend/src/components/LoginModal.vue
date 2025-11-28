<template>
  <div class="modal-overlay" @click="$emit('close')">
    <div class="modal" @click.stop>
      <h2>Login</h2>
      <form @submit.prevent="handleSubmit">
        <div class="form-group">
          <label>Username</label>
          <input v-model="username" type="text" required />
        </div>
        <div class="form-group">
          <label>Password</label>
          <input v-model="password" type="password" required />
        </div>
        <div class="form-group">
          <label>Tenant ID (opcional)</label>
          <input v-model="tenantId" type="text" placeholder="default" />
        </div>
        <div class="form-actions">
          <button type="button" @click="$emit('close')" class="btn btn-secondary">Cancelar</button>
          <button type="submit" class="btn btn-primary">Entrar</button>
        </div>
        <div v-if="error" class="error">{{ error }}</div>
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
          username: username.value,
          password: password.value,
          tenantId: tenantId.value || 'default'
        })
      } catch (err) {
        error.value = 'Credenciais inválidas'
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
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  width: 90%;
  max-width: 400px;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-group input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
}

.error {
  color: red;
  margin-top: 1rem;
  text-align: center;
}
</style>


