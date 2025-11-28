<template>
  <div class="customers-page">
    <div class="page-header">
      <div class="header-content">
        <h1>👥 Clientes</h1>
        <p>Gerencie o cadastro de clientes</p>
      </div>
      <button @click="showCreateModal = true" class="btn btn-primary btn-large">
        <span>➕</span>
        <span>Novo Cliente</span>
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Carregando clientes...</p>
    </div>

    <!-- Customers Grid -->
    <div v-else-if="customersStore.customers.length > 0" class="customers-grid">
      <div v-for="customer in customersStore.customers" :key="customer.id" class="customer-card">
        <div class="customer-card-header">
          <div class="customer-badge">ID: {{ customer.id }}</div>
          <div class="customer-actions">
            <button @click="editCustomer(customer)" class="icon-btn" title="Editar">
              ✏️
            </button>
            <button @click="deleteCustomer(customer.id)" class="icon-btn icon-btn-danger" title="Excluir">
              🗑️
            </button>
          </div>
        </div>
        <div class="customer-card-body">
          <h3 class="customer-name">{{ customer.name }}</h3>
          <div class="customer-info">
            <div class="info-item">
              <span class="info-icon">📧</span>
              <span class="info-label">Email:</span>
              <span class="info-value">{{ customer.email }}</span>
            </div>
            <div v-if="customer.phone" class="info-item">
              <span class="info-icon">📞</span>
              <span class="info-label">Telefone:</span>
              <span class="info-value">{{ customer.phone }}</span>
            </div>
            <div v-if="customer.document" class="info-item">
              <span class="info-icon">🆔</span>
              <span class="info-label">Documento:</span>
              <span class="info-value">{{ customer.document }}</span>
            </div>
          </div>
        </div>
        <div class="customer-card-footer">
          <span class="customer-date">
            📅 {{ formatDate(customer.createdAt) }}
          </span>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <div class="empty-icon">👥</div>
      <h3>Nenhum cliente cadastrado</h3>
      <p>Comece criando seu primeiro cliente</p>
      <button @click="showCreateModal = true" class="btn btn-primary btn-large">
        Criar Cliente
      </button>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showCreateModal || editingCustomer" class="modal-overlay" @click="cancelEdit">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ editingCustomer ? 'Editar' : 'Novo' }} Cliente</h2>
          <button @click="cancelEdit" class="modal-close">×</button>
        </div>
        <form @submit.prevent="saveCustomer" class="modal-form">
          <div class="form-group">
            <label>Nome Completo *</label>
            <input 
              v-model="form.name" 
              type="text" 
              required 
              maxlength="200"
              placeholder="Ex: João da Silva"
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label>Email *</label>
            <input 
              v-model="form.email" 
              type="email" 
              required 
              maxlength="200"
              placeholder="Ex: joao@example.com"
              class="form-input"
            />
            <small class="form-hint">Email único por tenant</small>
          </div>
          <div class="form-group">
            <label>Telefone</label>
            <input 
              v-model="form.phone" 
              type="tel" 
              maxlength="20"
              placeholder="Ex: (11) 99999-9999"
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label>Documento (CPF/CNPJ)</label>
            <input 
              v-model="form.document" 
              type="text" 
              maxlength="20"
              placeholder="Ex: 123.456.789-00"
              class="form-input"
            />
          </div>
          <div class="form-actions">
            <button type="submit" :disabled="loading" class="btn btn-primary btn-large">
              {{ loading ? 'Salvando...' : 'Salvar Cliente' }}
            </button>
            <button type="button" @click="cancelEdit" class="btn btn-outline btn-large">
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import { useCustomersStore } from '../stores/customers'

export default {
  name: 'Customers',
  setup() {
    const customersStore = useCustomersStore()
    const loading = ref(false)
    const showCreateModal = ref(false)
    const editingCustomer = ref(null)
    const form = ref({ 
      name: '', 
      email: '', 
      phone: '', 
      document: '' 
    })

    const loadData = async () => {
      loading.value = true
      try {
        await customersStore.fetchCustomers()
      } finally {
        loading.value = false
      }
    }

    const editCustomer = (customer) => {
      editingCustomer.value = customer
      form.value = { 
        name: customer.name, 
        email: customer.email, 
        phone: customer.phone || '', 
        document: customer.document || '' 
      }
    }

    const cancelEdit = () => {
      showCreateModal.value = false
      editingCustomer.value = null
      form.value = { name: '', email: '', phone: '', document: '' }
    }

    const saveCustomer = async () => {
      loading.value = true
      try {
        if (editingCustomer.value) {
          await customersStore.updateCustomer(editingCustomer.value.id, form.value)
        } else {
          await customersStore.createCustomer(form.value)
        }
        cancelEdit()
      } catch (error) {
        alert('Erro ao salvar: ' + (error.response?.data?.message || error.message))
      } finally {
        loading.value = false
      }
    }

    const deleteCustomer = async (id) => {
      if (!confirm('Tem certeza que deseja excluir este cliente?')) {
        return
      }
      try {
        await customersStore.deleteCustomer(id)
      } catch (error) {
        alert('Erro ao excluir: ' + (error.response?.data?.message || error.message))
      }
    }

    const formatDate = (dateString) => {
      return new Date(dateString).toLocaleDateString('pt-BR')
    }

    onMounted(() => {
      loadData()
    })

    return {
      customersStore,
      loading,
      showCreateModal,
      editingCustomer,
      form,
      editCustomer,
      cancelEdit,
      saveCustomer,
      deleteCustomer,
      formatDate
    }
  }
}
</script>

<style scoped>
.customers-page {
  padding: 0;
}

/* Page Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
  padding: 1.5rem;
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  flex-wrap: wrap;
  gap: 1rem;
}

.header-content h1 {
  font-size: 2rem;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0 0 0.25rem 0;
}

.header-content p {
  color: var(--text-secondary);
  margin: 0;
}

/* Customers Grid */
.customers-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.customer-card {
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  box-shadow: var(--shadow-md);
  transition: all 0.3s;
  border: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.customer-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-xl);
  border-color: var(--primary-color);
}

.customer-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--bg-tertiary);
}

.customer-badge {
  background: var(--bg-tertiary);
  color: var(--text-secondary);
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-sm);
  font-size: 0.75rem;
  font-weight: 600;
}

.customer-actions {
  display: flex;
  gap: 0.5rem;
}

.icon-btn {
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: var(--radius-sm);
  transition: all 0.2s;
  line-height: 1;
}

.icon-btn:hover {
  background: var(--bg-tertiary);
  transform: scale(1.1);
}

.icon-btn-danger:hover {
  background: #fee2e2;
}

.customer-card-body {
  flex: 1;
}

.customer-name {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0 0 1rem 0;
}

.customer-info {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.info-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9375rem;
}

.info-icon {
  font-size: 1rem;
}

.info-label {
  color: var(--text-secondary);
  font-weight: 500;
  min-width: 80px;
}

.info-value {
  color: var(--text-primary);
  flex: 1;
}

.customer-card-footer {
  padding-top: 1rem;
  border-top: 1px solid var(--bg-tertiary);
}

.customer-date {
  font-size: 0.8125rem;
  color: var(--text-secondary);
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
  animation: fadeIn 0.2s;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.modal-content {
  background: var(--bg-primary);
  border-radius: var(--radius-xl);
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: var(--shadow-xl);
  animation: slideUp 0.3s;
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
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid var(--border-color);
}

.modal-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.modal-close {
  background: none;
  border: none;
  font-size: 2rem;
  color: var(--text-secondary);
  cursor: pointer;
  line-height: 1;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-sm);
  transition: all 0.2s;
}

.modal-close:hover {
  background: var(--bg-tertiary);
  color: var(--text-primary);
}

.modal-form {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: var(--text-primary);
}

.form-input {
  width: 100%;
  padding: 0.75rem;
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

.form-hint {
  display: block;
  margin-top: 0.25rem;
  font-size: 0.8125rem;
  color: var(--text-secondary);
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--border-color);
}

.btn-outline {
  background: var(--bg-primary);
  color: var(--primary-color);
  border: 2px solid var(--primary-color);
}

.btn-outline:hover {
  background: var(--bg-tertiary);
}

/* Loading & Empty States */
.loading-container,
.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.empty-state h3 {
  font-size: 1.5rem;
  color: var(--text-primary);
  margin-bottom: 0.5rem;
}

.empty-state p {
  color: var(--text-secondary);
  margin-bottom: 2rem;
}

/* Responsive */
@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }

  .header-content h1 {
    font-size: 1.75rem;
  }

  .customers-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }

  .customer-card {
    padding: 1.25rem;
  }

  .modal-content {
    max-width: 100%;
    margin: 1rem;
  }

  .form-actions {
    flex-direction: column;
  }

  .btn-large {
    width: 100%;
  }
}

@media (max-width: 480px) {
  .page-header {
    padding: 1rem;
  }

  .customers-grid {
    gap: 0.75rem;
  }

  .modal-header,
  .modal-form {
    padding: 1rem;
  }
}
</style>

