<template>
  <div class="skus">
    <div class="header">
      <h2>SKUs</h2>
      <button @click="$router.push('/create-sku')" class="btn btn-primary">Novo SKU</button>
    </div>

    <div v-if="loading" class="loading">Carregando...</div>

    <table v-else class="table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Produto</th>
          <th>Cor</th>
          <th>Tamanho</th>
          <th>Código SKU</th>
          <th>Criado em</th>
          <th>Ações</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="sku in stockStore.skus" :key="sku.id">
          <td>{{ sku.id }}</td>
          <td>{{ sku.productName || `Produto ${sku.productId}` }}</td>
          <td>{{ sku.color?.name || '-' }}</td>
          <td>{{ sku.size?.name || '-' }}</td>
          <td>{{ sku.skuCode || '-' }}</td>
          <td>{{ formatDate(sku.createdAt) }}</td>
          <td>
            <button @click="editSku(sku)" class="btn btn-sm">Editar</button>
            <button @click="deleteSku(sku.id)" class="btn btn-sm btn-danger">Excluir</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="showEditModal && editingSku" class="modal">
      <div class="modal-content">
        <h3>Editar SKU</h3>
        <form @submit.prevent="updateSku">
          <div class="form-group">
            <label>Código de Barras</label>
            <input v-model="editForm.barcode" type="text" />
          </div>
          <div class="form-actions">
            <button type="submit" class="btn btn-primary">Salvar</button>
            <button type="button" @click="cancelEdit" class="btn btn-secondary">Cancelar</button>
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
  name: 'Skus',
  setup() {
    const stockStore = useStockStore()
    const showEditModal = ref(false)
    const editingSku = ref(null)
    const editForm = ref({ barcode: '' })

    const loadData = async () => {
      await stockStore.fetchSkus()
    }

    const editSku = (sku) => {
      editingSku.value = sku
      editForm.value = { barcode: sku.barcode || '' }
      showEditModal.value = true
    }

    const cancelEdit = () => {
      showEditModal.value = false
      editingSku.value = null
      editForm.value = { barcode: '' }
    }

    const updateSku = async () => {
      try {
        await stockStore.updateSku(editingSku.value.id, editForm.value)
        cancelEdit()
      } catch (error) {
        alert('Erro ao atualizar: ' + (error.response?.data?.message || error.message))
      }
    }

    const deleteSku = async (id) => {
      if (!confirm('Tem certeza que deseja excluir este SKU?')) return
      try {
        await stockStore.deleteSku(id)
      } catch (error) {
        alert('Erro ao excluir: ' + (error.response?.data?.message || error.message))
      }
    }

    const formatDate = (dateString) => {
      if (!dateString) return '-'
      return new Date(dateString).toLocaleString('pt-BR')
    }

    onMounted(() => {
      loadData()
    })

    return {
      stockStore,
      loading: stockStore.loading,
      showEditModal,
      editingSku,
      editForm,
      editSku,
      cancelEdit,
      updateSku,
      deleteSku,
      formatDate
    }
  }
}
</script>

<style scoped>
.skus {
  padding: 2rem 0;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.table {
  width: 100%;
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.table th,
.table td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid #eee;
}

.table th {
  background: #f8f9fa;
  font-weight: 600;
}

.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;
}

.btn-primary {
  background: #2563eb;
  color: white;
}

.btn-sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

.btn-danger {
  background: #dc2626;
  color: white;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  min-width: 400px;
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
  box-sizing: border-box;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
}

.loading {
  text-align: center;
  padding: 2rem;
}
</style>

