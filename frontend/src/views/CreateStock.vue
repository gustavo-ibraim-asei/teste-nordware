<template>
  <div class="create-stock">
    <div class="header">
      <h2>Associar SKU ao Estoque</h2>
      <button @click="$router.push('/stocks')" class="btn btn-secondary">Voltar</button>
    </div>

    <form @submit.prevent="createStock" class="stock-form">
      <div class="form-group">
        <label>SKU *</label>
        <select v-model="form.skuId" required>
          <option value="">Selecione um SKU</option>
          <option v-for="sku in stockStore.skus" :key="sku.id" :value="sku.id">
            {{ sku.productName || `Produto ${sku.productId}` }} - {{ sku.color?.name || 'N/A' }} / {{ sku.size?.name || 'N/A' }}
          </option>
        </select>
      </div>

      <div class="form-group">
        <label>Filial *</label>
        <select v-model="form.stockOfficeId" required>
          <option value="">Selecione uma filial</option>
          <option v-for="office in stockStore.stockOffices" :key="office.id" :value="office.id">
            {{ office.name }} ({{ office.code }})
          </option>
        </select>
      </div>

      <div class="form-group">
        <label>Quantidade *</label>
        <input v-model.number="form.quantity" type="number" min="1" required />
      </div>

      <div v-if="existingStock" class="warning-message">
        ⚠️ Já existe estoque para este SKU nesta filial (ID: {{ existingStock.id }}, Quantidade: {{ existingStock.quantity }})
      </div>

      <div class="form-actions">
        <button type="submit" :disabled="loading || !!existingStock" class="btn btn-primary">
          {{ loading ? 'Criando...' : 'Criar Estoque' }}
        </button>
        <button type="button" @click="$router.push('/stocks')" class="btn btn-secondary">Cancelar</button>
      </div>
    </form>

    <div v-if="error" class="error-message">{{ error }}</div>
    <div v-if="success" class="success-message">Estoque criado com sucesso! ID: {{ success }}</div>
  </div>
</template>

<script>
import { ref, computed, watch, onMounted } from 'vue'
import { useStockStore } from '../stores/stock'
import { useRouter } from 'vue-router'

export default {
  name: 'CreateStock',
  setup() {
    const stockStore = useStockStore()
    const router = useRouter()
    const loading = ref(false)
    const error = ref('')
    const success = ref('')

    const form = ref({
      skuId: null,
      stockOfficeId: null,
      quantity: 1
    })

    const existingStock = ref(null)

    const checkExistingStock = async () => {
      if (!form.value.skuId || !form.value.stockOfficeId) {
        existingStock.value = null
        return
      }

      try {
        // Carregar estoques para verificar se já existe
        await stockStore.fetchStocks(form.value.skuId, form.value.stockOfficeId)
        
        const found = stockStore.stocks.find(
          stock => 
            stock.skuId === form.value.skuId &&
            stock.stockOfficeId === form.value.stockOfficeId
        )
        
        existingStock.value = found || null
        
        // Limpar mensagem de sucesso se encontrar estoque existente
        if (found) {
          success.value = ''
        }
      } catch (err) {
        console.error('Erro ao verificar estoque existente:', err)
        existingStock.value = null
      }
    }

    watch(() => form.value.skuId, async (newSkuId) => {
      existingStock.value = null
      if (newSkuId) {
        try {
          await stockStore.fetchStocks(newSkuId, null)
          // Verificar se já existe após carregar
          if (form.value.stockOfficeId) {
            await checkExistingStock()
          }
        } catch (err) {
          console.error('Erro ao carregar estoques:', err)
        }
      }
    })

    watch(() => form.value.stockOfficeId, async () => {
      if (form.value.skuId && form.value.stockOfficeId) {
        await checkExistingStock()
      } else {
        existingStock.value = null
      }
    })

    const createStock = async () => {
      // Verificar novamente antes de criar
      await checkExistingStock()
      
      if (existingStock.value) {
        error.value = 'Já existe estoque para este SKU nesta filial'
        success.value = ''
        return
      }

      loading.value = true
      error.value = ''
      success.value = ''

      try {
        const result = await stockStore.createStock({
          skuId: form.value.skuId,
          stockOfficeId: form.value.stockOfficeId,
          quantity: form.value.quantity
        })
        success.value = result.id
        existingStock.value = null // Limpar verificação após criar com sucesso
        setTimeout(() => {
          router.push('/stocks')
        }, 2000)
      } catch (err) {
        const errorMessage = err.response?.data?.message || err.message || 'Erro ao criar estoque'
        error.value = errorMessage
        
        // Se o erro for de duplicidade, atualizar existingStock
        if (errorMessage.includes('já existe') || errorMessage.includes('duplicado')) {
          await checkExistingStock()
        }
        
        success.value = ''
      } finally {
        loading.value = false
      }
    }

    onMounted(async () => {
      try {
        await Promise.all([
          stockStore.fetchSkus(),
          stockStore.fetchStockOffices(),
          stockStore.fetchStocks()
        ])
      } catch (err) {
        error.value = 'Erro ao carregar dados: ' + (err.response?.data?.message || err.message)
      }
    })

    return {
      stockStore,
      form,
      loading,
      error,
      success,
      existingStock,
      createStock
    }
  }
}
</script>

<style scoped>
.create-stock {
  padding: 2rem 0;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.stock-form {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  max-width: 600px;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-group select,
.form-group input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.warning-message {
  padding: 1rem;
  background: #fef3c7;
  color: #92400e;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
}

.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
}

.btn-primary {
  background: #2563eb;
  color: white;
}

.btn-secondary {
  background: #6b7280;
  color: white;
}

.error-message {
  margin-top: 1rem;
  padding: 1rem;
  background: #fee2e2;
  color: #991b1b;
  border-radius: 4px;
}

.success-message {
  margin-top: 1rem;
  padding: 1rem;
  background: #d1fae5;
  color: #065f46;
  border-radius: 4px;
}
</style>

