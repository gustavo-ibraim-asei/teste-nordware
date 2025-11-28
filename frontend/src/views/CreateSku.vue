<template>
  <div class="create-sku">
    <div class="header">
      <h2>Criar SKU</h2>
      <button @click="$router.push('/skus')" class="btn btn-secondary">Voltar</button>
    </div>

    <form @submit.prevent="createSku" class="sku-form">
      <div class="form-group">
        <label>Produto *</label>
        <select v-model="form.productId" @change="loadSkusForProduct" required>
          <option value="">Selecione um produto</option>
          <option v-for="product in stockStore.products" :key="product.id" :value="product.id">
            {{ product.name }} ({{ product.code }})
          </option>
        </select>
      </div>

      <div class="form-group">
        <label>Cor *</label>
        <select v-model="form.colorId" required>
          <option value="">Selecione uma cor</option>
          <option v-for="color in stockStore.colors" :key="color.id" :value="color.id">
            {{ color.name }}
          </option>
        </select>
      </div>

      <div class="form-group">
        <label>Tamanho *</label>
        <select v-model="form.sizeId" required>
          <option value="">Selecione um tamanho</option>
          <option v-for="size in stockStore.sizes" :key="size.id" :value="size.id">
            {{ size.name }}
          </option>
        </select>
      </div>

      <div v-if="existingSku" class="warning-message">
        ⚠️ Já existe um SKU para este produto com esta cor e tamanho (ID: {{ existingSku.id }})
      </div>

      <div class="form-actions">
        <button type="submit" :disabled="loading || !!existingSku" class="btn btn-primary">
          {{ loading ? 'Criando...' : 'Criar SKU' }}
        </button>
        <button type="button" @click="$router.push('/skus')" class="btn btn-secondary">Cancelar</button>
      </div>
    </form>

    <div v-if="error" class="error-message">{{ error }}</div>
    <div v-if="success" class="success-message">SKU criado com sucesso! ID: {{ success }}</div>
  </div>
</template>

<script>
import { ref, computed, watch, onMounted } from 'vue'
import { useStockStore } from '../stores/stock'
import { useRouter } from 'vue-router'

export default {
  name: 'CreateSku',
  setup() {
    const stockStore = useStockStore()
    const router = useRouter()
    const loading = ref(false)
    const error = ref('')
    const success = ref('')

    const form = ref({
      productId: null,
      colorId: null,
      sizeId: null
    })

    const existingSku = ref(null)

    const checkExistingSku = async () => {
      if (!form.value.productId || !form.value.colorId || !form.value.sizeId) {
        existingSku.value = null
        return
      }

      try {
        // Carregar SKUs do produto para verificar se já existe
        await stockStore.fetchSkus(form.value.productId)
        
        const found = stockStore.skus.find(
          sku => 
            sku.productId === form.value.productId &&
            sku.colorId === form.value.colorId &&
            sku.sizeId === form.value.sizeId
        )
        
        existingSku.value = found || null
        
        // Limpar mensagem de sucesso se encontrar SKU existente
        if (found) {
          success.value = ''
        }
      } catch (err) {
        console.error('Erro ao verificar SKU existente:', err)
        existingSku.value = null
      }
    }

    const loadSkusForProduct = async () => {
      if (form.value.productId) {
        try {
          await stockStore.fetchSkus(form.value.productId)
          // Verificar se já existe após carregar
          await checkExistingSku()
        } catch (err) {
          console.error('Erro ao carregar SKUs:', err)
        }
      }
    }

    // Watchers para verificar quando os campos mudarem
    watch(() => form.value.productId, () => {
      form.value.colorId = null
      form.value.sizeId = null
      existingSku.value = null
    })

    watch(() => form.value.colorId, () => {
      form.value.sizeId = null
      existingSku.value = null
    })

    watch(() => [form.value.productId, form.value.colorId, form.value.sizeId], () => {
      if (form.value.productId && form.value.colorId && form.value.sizeId) {
        checkExistingSku()
      }
    })

    const createSku = async () => {
      // Verificar novamente antes de criar
      await checkExistingSku()
      
      if (existingSku.value) {
        error.value = 'Já existe um SKU com esta combinação de produto, cor e tamanho'
        success.value = ''
        return
      }

      loading.value = true
      error.value = ''
      success.value = ''

      try {
        const result = await stockStore.createSku({
          productId: form.value.productId,
          colorId: form.value.colorId,
          sizeId: form.value.sizeId
        })
        success.value = result.id
        existingSku.value = null // Limpar verificação após criar com sucesso
        setTimeout(() => {
          router.push('/skus')
        }, 2000)
      } catch (err) {
        const errorMessage = err.response?.data?.message || err.message || 'Erro ao criar SKU'
        error.value = errorMessage
        
        // Se o erro for de duplicidade, atualizar existingSku
        if (errorMessage.includes('já existe') || errorMessage.includes('já existe')) {
          await checkExistingSku()
        }
        
        success.value = ''
      } finally {
        loading.value = false
      }
    }

    onMounted(async () => {
      try {
        await Promise.all([
          stockStore.fetchProducts(),
          stockStore.fetchColors(),
          stockStore.fetchSizes(),
          stockStore.fetchSkus()
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
      existingSku,
      loadSkusForProduct,
      createSku
    }
  }
}
</script>

<style scoped>
.create-sku {
  padding: 2rem 0;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.sku-form {
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

.form-group select {
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

.btn-primary:disabled {
  background: #9ca3af;
  cursor: not-allowed;
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

