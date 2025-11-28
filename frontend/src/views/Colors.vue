<template>
  <div class="colors">
    <div class="header">
      <h2>Cores</h2>
      <button @click="showCreateModal = true" class="btn btn-primary">Nova Cor</button>
    </div>

    <div v-if="loading" class="loading">Carregando...</div>

    <table v-else class="table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Nome</th>
          <th>Código</th>
          <th>Ações</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="color in stockStore.colors" :key="color.id">
          <td>{{ color.id }}</td>
          <td>{{ color.name }}</td>
          <td>{{ color.code || '-' }}</td>
          <td>
            <button @click="editColor(color)" class="btn btn-sm">Editar</button>
            <button @click="deleteColor(color.id)" class="btn btn-sm btn-danger">Excluir</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="showCreateModal || editingColor" class="modal">
      <div class="modal-content">
        <h3>{{ editingColor ? 'Editar' : 'Nova' }} Cor</h3>
        <form @submit.prevent="saveColor">
          <div class="form-group">
            <label>Nome *</label>
            <input v-model="form.name" type="text" required />
          </div>
          <div class="form-group">
            <label>Código</label>
            <input v-model="form.code" type="text" />
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
  name: 'Colors',
  setup() {
    const stockStore = useStockStore()
    const showCreateModal = ref(false)
    const editingColor = ref(null)
    const form = ref({ name: '', code: '' })

    const loadData = async () => {
      await stockStore.fetchColors()
    }

    const editColor = (color) => {
      editingColor.value = color
      form.value = { name: color.name, code: color.code || '' }
    }

    const cancelEdit = () => {
      showCreateModal.value = false
      editingColor.value = null
      form.value = { name: '', code: '' }
    }

    const saveColor = async () => {
      try {
        if (editingColor.value) {
          await stockStore.updateColor(editingColor.value.id, form.value)
        } else {
          await stockStore.createColor(form.value)
        }
        cancelEdit()
      } catch (error) {
        alert('Erro ao salvar: ' + (error.response?.data?.message || error.message))
      }
    }

    const deleteColor = async (id) => {
      if (!confirm('Tem certeza que deseja excluir esta cor?')) return
      try {
        await stockStore.deleteColor(id)
      } catch (error) {
        alert('Erro ao excluir: ' + (error.response?.data?.message || error.message))
      }
    }

    onMounted(() => {
      loadData()
    })

    return {
      stockStore,
      loading: stockStore.loading,
      showCreateModal,
      editingColor,
      form,
      editColor,
      cancelEdit,
      saveColor,
      deleteColor
    }
  }
}
</script>

<style scoped>
.colors {
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



