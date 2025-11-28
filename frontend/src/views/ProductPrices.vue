<template>
  <div class="product-prices">
    <div class="header">
      <h2>Preços de Produtos</h2>
      <button @click="showCreateModal = true" class="btn btn-primary">Novo Preço</button>
    </div>

    <div class="filters">
      <select v-model="filterProductId" @change="loadData">
        <option value="">Todos os produtos</option>
        <option v-for="product in stockStore.products" :key="product.id" :value="product.id">
          {{ product.name }}
        </option>
      </select>
      <select v-model="filterPriceTableId" @change="loadData">
        <option value="">Todas as tabelas</option>
        <option v-for="priceTable in pricesStore.priceTables" :key="priceTable.id" :value="priceTable.id">
          {{ priceTable.name }}
        </option>
      </select>
    </div>

    <div v-if="loading" class="loading">Carregando...</div>

    <table v-else class="table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Produto</th>
          <th>Tabela de Preços</th>
          <th>Preço Unitário</th>
          <th>Criado em</th>
          <th>Ações</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="productPrice in pricesStore.productPrices" :key="productPrice.id">
          <td>{{ productPrice.id }}</td>
          <td>{{ productPrice.productName }}</td>
          <td>{{ productPrice.priceTableName }}</td>
          <td>R$ {{ productPrice.unitPrice.toFixed(2) }}</td>
          <td>{{ formatDate(productPrice.createdAt) }}</td>
          <td>
            <button @click="editProductPrice(productPrice)" class="btn btn-sm">Editar</button>
            <button @click="deleteProductPrice(productPrice.id)" class="btn btn-sm btn-danger">Excluir</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="showCreateModal || editingProductPrice" class="modal">
      <div class="modal-content">
        <h3>{{ editingProductPrice ? 'Editar' : 'Novo' }} Preço de Produto</h3>
        <form @submit.prevent="saveProductPrice">
          <div class="form-group">
            <label>Produto *</label>
            <select v-model="form.productId" required :disabled="!!editingProductPrice">
              <option value="">Selecione um produto</option>
              <option v-for="product in stockStore.products" :key="product.id" :value="product.id">
                {{ product.name }} ({{ product.code }})
              </option>
            </select>
          </div>
          <div class="form-group">
            <label>Tabela de Preços *</label>
            <select v-model="form.priceTableId" required :disabled="!!editingProductPrice">
              <option value="">Selecione uma tabela</option>
              <option v-for="priceTable in pricesStore.priceTables" :key="priceTable.id" :value="priceTable.id">
                {{ priceTable.name }}
              </option>
            </select>
          </div>
          <div class="form-group">
            <label>Preço Unitário *</label>
            <input v-model.number="form.unitPrice" type="number" step="0.01" min="0" required />
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
import { useStockStore } from '../stores/stock'

export default {
  name: 'ProductPrices',
  setup() {
    const pricesStore = usePricesStore()
    const stockStore = useStockStore()
    const showCreateModal = ref(false)
    const editingProductPrice = ref(null)
    const filterProductId = ref('')
    const filterPriceTableId = ref('')
    const form = ref({ productId: null, priceTableId: null, unitPrice: 0 })

    const loadData = async () => {
      await Promise.all([
        pricesStore.fetchProductPrices(
          filterProductId.value || null, 
          filterPriceTableId.value || null
        ),
        stockStore.fetchProducts(),
        pricesStore.fetchPriceTables()
      ])
    }

    const editProductPrice = (productPrice) => {
      editingProductPrice.value = productPrice
      form.value = { 
        productId: productPrice.productId,
        priceTableId: productPrice.priceTableId,
        unitPrice: productPrice.unitPrice
      }
    }

    const cancelEdit = () => {
      showCreateModal.value = false
      editingProductPrice.value = null
      form.value = { productId: null, priceTableId: null, unitPrice: 0 }
    }

    const saveProductPrice = async () => {
      try {
        if (editingProductPrice.value) {
          await pricesStore.updateProductPrice(editingProductPrice.value.id, {
            unitPrice: form.value.unitPrice
          })
        } else {
          await pricesStore.createProductPrice({
            productId: form.value.productId,
            priceTableId: form.value.priceTableId,
            unitPrice: form.value.unitPrice
          })
        }
        cancelEdit()
        await loadData()
      } catch (error) {
        alert('Erro ao salvar: ' + (error.response?.data?.message || error.message))
      }
    }

    const deleteProductPrice = async (id) => {
      if (!confirm('Tem certeza que deseja excluir este preço?')) return
      try {
        await pricesStore.deleteProductPrice(id)
        await loadData()
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
      stockStore,
      loading: pricesStore.loading,
      showCreateModal,
      editingProductPrice,
      filterProductId,
      filterPriceTableId,
      form,
      loadData,
      editProductPrice,
      cancelEdit,
      saveProductPrice,
      deleteProductPrice,
      formatDate
    }
  }
}
</script>

<style scoped>
.product-prices {
  padding: 2rem 0;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 4px;
}

.filters select {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  min-width: 200px;
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
.form-group select {
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

