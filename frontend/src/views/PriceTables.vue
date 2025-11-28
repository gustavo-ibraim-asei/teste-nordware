<template>
  <div class="price-tables">
    <div class="header">
      <h2>Tabelas de Preços</h2>
      <button @click="showCreateModal = true" class="btn btn-primary">Nova Tabela</button>
    </div>

    <div class="filters">
      <label>
        <input type="checkbox" v-model="onlyActive" @change="loadData" />
        Mostrar apenas ativas
      </label>
    </div>

    <div v-if="loading" class="loading">Carregando...</div>

    <table v-else class="table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Nome</th>
          <th>Descrição</th>
          <th>Status</th>
          <th>Criado em</th>
          <th>Ações</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="priceTable in pricesStore.priceTables" :key="priceTable.id">
          <td>{{ priceTable.id }}</td>
          <td>{{ priceTable.name }}</td>
          <td>{{ priceTable.description || '-' }}</td>
          <td>
            <span :class="['status-badge', priceTable.isActive ? 'active' : 'inactive']">
              {{ priceTable.isActive ? 'Ativa' : 'Inativa' }}
            </span>
          </td>
          <td>{{ formatDate(priceTable.createdAt) }}</td>
          <td>
            <button @click="editPriceTable(priceTable)" class="btn btn-sm">Editar</button>
            <button @click="deletePriceTable(priceTable.id)" class="btn btn-sm btn-danger">Excluir</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="showCreateModal || editingPriceTable" class="modal">
      <div class="modal-content">
        <h3>{{ editingPriceTable ? 'Editar' : 'Nova' }} Tabela de Preços</h3>
        <form @submit.prevent="savePriceTable">
          <div class="form-group">
            <label>Nome *</label>
            <input v-model="form.name" type="text" required maxlength="200" />
          </div>
          <div class="form-group">
            <label>Descrição</label>
            <textarea v-model="form.description" maxlength="1000" rows="3"></textarea>
          </div>
          <div v-if="editingPriceTable" class="form-group">
            <label>
              <input type="checkbox" v-model="form.isActive" />
              Ativa
            </label>
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
import { usePricesStore } from '../stores/prices'

export default {
  name: 'PriceTables',
  setup() {
    const pricesStore = usePricesStore()
    const showCreateModal = ref(false)
    const editingPriceTable = ref(null)
    const onlyActive = ref(false)
    const form = ref({ name: '', description: '', isActive: true })

    const loadData = async () => {
      await pricesStore.fetchPriceTables(onlyActive.value ? true : null)
    }

    const editPriceTable = (priceTable) => {
      editingPriceTable.value = priceTable
      form.value = { 
        name: priceTable.name, 
        description: priceTable.description || '', 
        isActive: priceTable.isActive 
      }
    }

    const cancelEdit = () => {
      showCreateModal.value = false
      editingPriceTable.value = null
      form.value = { name: '', description: '', isActive: true }
    }

    const savePriceTable = async () => {
      try {
        if (editingPriceTable.value) {
          await pricesStore.updatePriceTable(editingPriceTable.value.id, form.value)
        } else {
          await pricesStore.createPriceTable({
            name: form.value.name,
            description: form.value.description
          })
        }
        cancelEdit()
      } catch (error) {
        alert('Erro ao salvar: ' + (error.response?.data?.message || error.message))
      }
    }

    const deletePriceTable = async (id) => {
      if (!confirm('Tem certeza que deseja excluir esta tabela de preços?')) return
      try {
        await pricesStore.deletePriceTable(id)
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
      pricesStore,
      loading: pricesStore.loading,
      showCreateModal,
      editingPriceTable,
      onlyActive,
      form,
      loadData,
      editPriceTable,
      cancelEdit,
      savePriceTable,
      deletePriceTable,
      formatDate
    }
  }
}
</script>

<style scoped>
.price-tables {
  padding: 2rem 0;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.filters {
  margin-bottom: 1rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 4px;
}

.filters label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
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

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.875rem;
  font-weight: 500;
}

.status-badge.active {
  background: #d1fae5;
  color: #065f46;
}

.status-badge.inactive {
  background: #fee2e2;
  color: #991b1b;
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

.btn-secondary {
  background: #6c757d;
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

.form-group input,
.form-group textarea {
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

