<template>
  <div class="products-page">
    <div class="page-header">
      <div class="header-content">
        <h1>Produtos</h1>
        <p>Gerencie seu catálogo de produtos</p>
      </div>
      <button @click="showCreateModal = true" class="btn btn-primary btn-large">
        <span>➕</span>
        <span>Novo Produto</span>
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Carregando produtos...</p>
    </div>

    <!-- Products Grid -->
    <div v-else-if="stockStore.products.length > 0" class="products-grid">
      <div v-for="product in stockStore.products" :key="product.id" class="product-card">
        <div class="product-card-header">
          <div class="product-badge">ID: {{ product.id }}</div>
          <div class="product-actions">
            <button @click="editProduct(product)" class="icon-btn" title="Editar">
              ✏️
            </button>
            <button @click="deleteProduct(product.id)" class="icon-btn icon-btn-danger" title="Excluir">
              🗑️
            </button>
          </div>
        </div>
        <div class="product-card-body">
          <h3 class="product-name">{{ product.name }}</h3>
          <div class="product-code">
            <span class="code-label">Código:</span>
            <span class="code-value">{{ product.code }}</span>
          </div>
          <p v-if="product.description" class="product-description">
            {{ product.description }}
          </p>
          <div v-else class="product-description-empty">
            Sem descrição
          </div>
        </div>
        <div class="product-card-footer">
          <span class="product-date">
            📅 {{ formatDate(product.createdAt) }}
          </span>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <div class="empty-icon">📦</div>
      <h3>Nenhum produto cadastrado</h3>
      <p>Comece criando seu primeiro produto</p>
      <button @click="showCreateModal = true" class="btn btn-primary btn-large">
        Criar Produto
      </button>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showCreateModal || editingProduct" class="modal-overlay" @click="cancelEdit">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ editingProduct ? 'Editar' : 'Novo' }} Produto</h2>
          <button @click="cancelEdit" class="modal-close">×</button>
        </div>
        <form @submit.prevent="saveProduct" class="modal-form">
          <div class="form-group">
            <label>Nome do Produto *</label>
            <input 
              v-model="form.name" 
              type="text" 
              required 
              maxlength="200"
              placeholder="Ex: Camiseta Básica"
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label>Código do Produto *</label>
            <input 
              v-model="form.code" 
              type="text" 
              required 
              maxlength="50"
              placeholder="Ex: CAM001"
              class="form-input"
            />
            <small class="form-hint">Código único para identificação</small>
          </div>
          <div class="form-group">
            <label>Descrição</label>
            <textarea 
              v-model="form.description" 
              maxlength="1000" 
              rows="4"
              placeholder="Descreva o produto..."
              class="form-textarea"
            ></textarea>
            <small class="form-hint">{{ form.description?.length || 0 }}/1000 caracteres</small>
          </div>
          <div class="form-actions">
            <button type="submit" :disabled="loading" class="btn btn-primary btn-large">
              {{ loading ? 'Salvando...' : 'Salvar Produto' }}
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
import { useStockStore } from '../stores/stock'

export default {
  name: 'Products',
  setup() {
    const stockStore = useStockStore()
    const loading = ref(false)
    const showCreateModal = ref(false)
    const editingProduct = ref(null)
    const form = ref({ name: '', code: '', description: '' })

    const loadData = async () => {
      loading.value = true
      try {
        await stockStore.fetchProducts()
      } finally {
        loading.value = false
      }
    }

    const editProduct = (product) => {
      editingProduct.value = product
      form.value = { 
        name: product.name, 
        code: product.code, 
        description: product.description || '' 
      }
    }

    const cancelEdit = () => {
      showCreateModal.value = false
      editingProduct.value = null
      form.value = { name: '', code: '', description: '' }
    }

    const saveProduct = async () => {
      loading.value = true
      try {
        if (editingProduct.value) {
          await stockStore.updateProduct(editingProduct.value.id, form.value)
        } else {
          await stockStore.createProduct(form.value)
        }
        cancelEdit()
      } catch (error) {
        alert('Erro ao salvar: ' + (error.response?.data?.message || error.message))
      } finally {
        loading.value = false
      }
    }

    const deleteProduct = async (id) => {
      if (!confirm('Tem certeza que deseja excluir este produto?')) {
        return
      }
      try {
        await stockStore.deleteProduct(id)
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
      stockStore,
      loading,
      showCreateModal,
      editingProduct,
      form,
      editProduct,
      cancelEdit,
      saveProduct,
      deleteProduct,
      formatDate
    }
  }
}
</script>

<style scoped>
.products-page {
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

/* Products Grid */
.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.5rem;
}

.product-card {
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

.product-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-xl);
  border-color: var(--primary-color);
}

.product-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--bg-tertiary);
}

.product-badge {
  background: var(--bg-tertiary);
  color: var(--text-secondary);
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-sm);
  font-size: 0.75rem;
  font-weight: 600;
}

.product-actions {
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

.product-card-body {
  flex: 1;
}

.product-name {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0 0 0.75rem 0;
}

.product-code {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}

.code-label {
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.code-value {
  font-size: 0.9375rem;
  font-weight: 600;
  color: var(--primary-color);
  background: var(--bg-tertiary);
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-sm);
}

.product-description {
  color: var(--text-secondary);
  font-size: 0.9375rem;
  line-height: 1.5;
  margin: 0;
}

.product-description-empty {
  color: var(--text-secondary);
  font-size: 0.875rem;
  font-style: italic;
  opacity: 0.6;
}

.product-card-footer {
  padding-top: 1rem;
  border-top: 1px solid var(--bg-tertiary);
}

.product-date {
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

.form-input,
.form-textarea {
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

.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.form-textarea {
  resize: vertical;
  min-height: 100px;
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

  .products-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }

  .product-card {
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

  .products-grid {
    gap: 0.75rem;
  }

  .modal-header,
  .modal-form {
    padding: 1rem;
  }
}
</style>
