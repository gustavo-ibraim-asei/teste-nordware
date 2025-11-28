<template>
  <div class="create-order">
    <h2>Criar Pedido</h2>

    <form @submit.prevent="createOrder" class="order-form">
      <div class="form-group">
        <label>Cliente ID *</label>
        <input v-model.number="form.customerId" type="number" required />
      </div>

      <div class="form-group">
        <label>Endereço de Entrega</label>
        <div class="address-grid">
          <div class="cep-input-group">
            <label>CEP *</label>
            <input 
              v-model="form.shippingAddress.zipCode" 
              placeholder="00000-000" 
              required 
              @blur="onCepBlur"
              @input="onCepInput"
              :disabled="consultandoCep"
              maxlength="9"
            />
          </div>
          <input v-model="form.shippingAddress.street" placeholder="Rua" required />
          <input v-model="form.shippingAddress.number" placeholder="Número" required />
          <input v-model="form.shippingAddress.complement" placeholder="Complemento" />
          <input v-model="form.shippingAddress.neighborhood" placeholder="Bairro" required />
          <input v-model="form.shippingAddress.city" placeholder="Cidade" required />
          <input v-model="form.shippingAddress.state" placeholder="Estado" required />
        </div>
        <div v-if="shippingOptions.length > 0" class="shipping-options">
          <h4>Opções de Frete:</h4>
          <div v-for="option in shippingOptions" :key="option.carrierId + '-' + option.shippingTypeId" class="shipping-option">
            <input 
              type="radio" 
              :id="`shipping-${option.carrierId}-${option.shippingTypeId}`"
              :value="option.price" 
              v-model="selectedShippingPrice"
              @change="updateShippingCost"
            />
            <label :for="`shipping-${option.carrierId}-${option.shippingTypeId}`">
              {{ option.description }} - R$ {{ option.price.toFixed(2) }} ({{ option.estimatedDays }} dias)
            </label>
          </div>
        </div>
      </div>

      <div class="items-section">
        <h3>Itens do Pedido</h3>
        <div class="price-table-selector">
          <label>Tabela de Preços *</label>
          <select v-model="selectedPriceTableId" @change="onPriceTableChange" required>
            <option value="">Selecione uma tabela de preços</option>
            <option v-for="priceTable in activePriceTables" :key="priceTable.id" :value="priceTable.id">
              {{ priceTable.name }}
            </option>
          </select>
        </div>
        <div v-if="loadingSkus" class="loading">Carregando produtos disponíveis...</div>
        <div class="items-header">
          <div class="header-cell">Produto</div>
          <div class="header-cell">Cor</div>
          <div class="header-cell">Tamanho</div>
          <div class="header-cell">Estoque</div>
          <div class="header-cell">Quantidade</div>
          <div class="header-cell">Preço Unitário</div>
          <div class="header-cell">Ações</div>
        </div>
        <div v-for="(item, index) in form.items" :key="index" class="item-row">
          <select v-model="item.productId" @change="onProductChange(index)" required class="product-select">
            <option value="">Selecione o Produto</option>
            <option 
              v-for="product in productsWithSkus" 
              :key="product.id" 
              :value="product.id">
              {{ product.name }} ({{ product.code }})
            </option>
          </select>
          <select v-model="item.colorId" @change="onColorChange(index)" required :disabled="!item.productId">
            <option value="">Cor</option>
            <option 
              v-for="color in getAvailableColors(item.productId)" 
              :key="color.id" 
              :value="color.id">
              {{ color.name }}
            </option>
          </select>
          <select v-model="item.sizeId" @change="onSizeChange(index)" required :disabled="!item.productId || !item.colorId">
            <option value="">Tamanho</option>
            <option 
              v-for="size in getAvailableSizes(item.productId, item.colorId)" 
              :key="size.id" 
              :value="size.id">
              {{ size.name }}
            </option>
          </select>
          <div v-if="item.skuId" class="sku-info">
            <span class="stock-info">Estoque: {{ getMaxQuantity(item.skuId) }}</span>
          </div>
          <input 
            v-model.number="item.quantity" 
            type="number" 
            placeholder="Quantidade" 
            required 
            min="1"
            :max="getMaxQuantity(item.skuId)"
            @input="validateQuantity(index)"
            :disabled="!item.skuId"
          />
          <input 
            v-model.number="item.unitPrice" 
            type="number" 
            step="0.01" 
            placeholder="Preço Unitário" 
            required 
            :disabled="!item.productId || !selectedPriceTableId"
          />
          <button type="button" @click="removeItem(index)" class="btn btn-danger btn-sm">Remover</button>
        </div>
        <button type="button" @click="addItem" class="btn btn-secondary">Adicionar Item</button>
      </div>

      <div class="form-actions">
        <button type="submit" :disabled="loading" class="btn btn-primary">
          {{ loading ? 'Criando...' : 'Criar Pedido' }}
        </button>
        <button type="button" @click="$router.push('/')" class="btn btn-secondary">Cancelar</button>
      </div>
    </form>

    <div v-if="error" class="error-message">{{ error }}</div>
    <div v-if="success" class="success-message">Pedido criado com sucesso! ID: {{ success }}</div>
  </div>
</template>

<script>
import { ref, onMounted, watch } from 'vue'
import { useOrdersStore } from '../stores/orders'
import { useStockStore } from '../stores/stock'
import { usePricesStore } from '../stores/prices'
import axios from 'axios'
import { useRouter } from 'vue-router'
import { API_BASE_URL } from '../config/api'

export default {
  name: 'CreateOrder',
  setup() {
    const ordersStore = useOrdersStore()
    const stockStore = useStockStore()
    const pricesStore = usePricesStore()
    const router = useRouter()
    const loading = ref(false)
    const error = ref('')
    const success = ref('')

    const loadingSkus = ref(false)
    const consultandoCep = ref(false)
    const calculatingShipping = ref(false)
    const shippingOptions = ref([])
    const selectedShippingPrice = ref(null)
    const productsWithSkus = ref([])
    const selectedPriceTableId = ref(null)
    const activePriceTables = ref([])
    
    const form = ref({
      customerId: null,
      shippingAddress: {
        street: '',
        number: '',
        complement: '',
        neighborhood: '',
        city: '',
        state: '',
        zipCode: ''
      },
      items: [
        {
          skuId: null,
          productId: null,
          colorId: null,
          sizeId: null,
          productName: '',
          quantity: 1,
          unitPrice: 0
        }
      ]
    })


    const getMaxQuantity = (skuId) => {
      if (!skuId) return 0
      const skuWithStock = stockStore.skusWithStock.find(s => s.sku.id === skuId)
      return skuWithStock ? skuWithStock.totalAvailableQuantity : 0
    }

    const getProductsWithSkus = () => {
      const productIds = new Set()
      stockStore.skusWithStock.forEach(skuWithStock => {
        productIds.add(skuWithStock.sku.productId)
      })
      return stockStore.products.filter(p => productIds.has(p.id))
    }

    const getAvailableColors = (productId) => {
      if (!productId) return []
      const colorIds = new Set()
      stockStore.skusWithStock.forEach(skuWithStock => {
        if (skuWithStock.sku.productId === productId && skuWithStock.totalAvailableQuantity > 0) {
          colorIds.add(skuWithStock.sku.colorId)
        }
      })
      return stockStore.colors.filter(c => colorIds.has(c.id))
    }

    const getAvailableSizes = (productId, colorId) => {
      if (!productId || !colorId) return []
      const sizeIds = new Set()
      stockStore.skusWithStock.forEach(skuWithStock => {
        if (
          skuWithStock.sku.productId === productId && 
          skuWithStock.sku.colorId === colorId &&
          skuWithStock.totalAvailableQuantity > 0
        ) {
          sizeIds.add(skuWithStock.sku.sizeId)
        }
      })
      return stockStore.sizes.filter(s => sizeIds.has(s.id))
    }

    const findSku = (productId, colorId, sizeId) => {
      return stockStore.skusWithStock.find(
        s => s.sku.productId === productId && 
             s.sku.colorId === colorId && 
             s.sku.sizeId === sizeId &&
             s.totalAvailableQuantity > 0
      )
    }

    const onProductChange = (index) => {
      const item = form.value.items[index]
      item.colorId = null
      item.sizeId = null
      item.skuId = null
      item.productName = ''
    }

    const onColorChange = (index) => {
      const item = form.value.items[index]
      item.sizeId = null
      item.skuId = null
      item.productName = ''
    }

    const onSizeChange = async (index) => {
      const item = form.value.items[index]
      if (item.productId && item.colorId && item.sizeId) {
        const skuWithStock = findSku(item.productId, item.colorId, item.sizeId)
        if (skuWithStock) {
          item.skuId = skuWithStock.sku.id
          const product = stockStore.products.find(p => p.id === item.productId)
          item.productName = `${product?.name || `Produto ${item.productId}`} - ${skuWithStock.sku.color?.name} / ${skuWithStock.sku.size?.name}`
          if (item.quantity > skuWithStock.totalAvailableQuantity) {
            item.quantity = skuWithStock.totalAvailableQuantity
          }
          
          // Carregar preço da tabela de preços
          if (selectedPriceTableId.value) {
            await loadProductPrice(index)
          }
        } else {
          item.skuId = null
          item.productName = ''
          error.value = 'SKU não encontrado ou sem estoque disponível'
        }
      }
      calculateShipping()
    }

    const loadProductPrice = async (index) => {
      const item = form.value.items[index]
      if (!item.productId || !selectedPriceTableId.value) return

      try {
        const productPrice = await pricesStore.getProductPrice(item.productId, selectedPriceTableId.value)
        if (productPrice) {
          item.unitPrice = productPrice.unitPrice
        } else {
          // Se não encontrar preço, manter o valor atual ou definir como 0
          if (!item.unitPrice) {
            item.unitPrice = 0
          }
        }
      } catch (err) {
        console.error('Erro ao carregar preço:', err)
        // Não definir erro aqui, apenas logar
      }
    }

    const onPriceTableChange = async () => {
      // Recarregar preços de todos os itens quando a tabela de preços mudar
      for (let i = 0; i < form.value.items.length; i++) {
        if (form.value.items[i].productId) {
          await loadProductPrice(i)
        }
      }
    }

    const validateQuantity = (index) => {
      const item = form.value.items[index]
      const maxQty = getMaxQuantity(item.skuId)
      if (item.quantity > maxQty) {
        item.quantity = maxQty
        alert(`Quantidade máxima disponível: ${maxQty}`)
      }
    }

    const formatCep = (value) => {
      const numbers = value.replace(/\D/g, '')
      if (numbers.length <= 5) return numbers
      return `${numbers.slice(0, 5)}-${numbers.slice(5, 8)}`
    }

    const onCepInput = (event) => {
      const formatted = formatCep(event.target.value)
      form.value.shippingAddress.zipCode = formatted
      
      // Calcular frete quando atingir 8 caracteres (sem contar o hífen)
      const numbers = formatted.replace(/\D/g, '')
      if (numbers.length === 8) {
        calculateShipping()
      }
    }

    const onCepBlur = () => {
      const cep = form.value.shippingAddress.zipCode?.replace(/\D/g, '')
      if (cep && cep.length === 8) {
        consultarCep()
        calculateShipping()
      }
    }

    const consultarCep = async () => {
      const cep = form.value.shippingAddress.zipCode?.replace(/\D/g, '')
      if (!cep || cep.length !== 8) {
        return
      }

      consultandoCep.value = true
      error.value = ''
      try {
        const token = localStorage.getItem('token')
        const response = await axios.get(
          `${API_BASE_URL}/cep/${cep}`,
          {
            headers: {
              Authorization: `Bearer ${token}`,
              'X-Tenant-Id': localStorage.getItem('tenantId') || 'default'
            }
          }
        )

        if (response.data) {
          form.value.shippingAddress.street = response.data.logradouro || ''
          form.value.shippingAddress.neighborhood = response.data.bairro || ''
          form.value.shippingAddress.city = response.data.localidade || ''
          form.value.shippingAddress.state = response.data.uf || ''
          form.value.shippingAddress.complement = response.data.complemento || ''
          form.value.shippingAddress.zipCode = formatCep(response.data.cep || form.value.shippingAddress.zipCode)
        }
      } catch (err) {
        if (err.response?.status === 404) {
          // CEP não encontrado, mas não é erro crítico
          console.warn('CEP não encontrado')
        } else {
          error.value = 'Erro ao consultar CEP: ' + (err.response?.data?.message || err.message)
        }
      } finally {
        consultandoCep.value = false
      }
    }

    const calculateShipping = async () => {
      const cep = form.value.shippingAddress.zipCode?.replace(/\D/g, '')
      if (!cep || cep.length !== 8 || form.value.items.length === 0) {
        shippingOptions.value = []
        return
      }

      calculatingShipping.value = true
      try {
        const orderTotal = form.value.items.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0)
        const totalWeight = form.value.items.reduce((sum, item) => sum + (item.quantity * 0.5), 0) // Assumindo 0.5kg por item

        const token = localStorage.getItem('token')
        const response = await axios.post(
          `${API_BASE_URL}/shipping/calculate`,
          {
            zipCode: cep,
            orderTotal: orderTotal,
            totalWeight: totalWeight
          },
          {
            headers: {
              Authorization: `Bearer ${token}`,
              'X-Tenant-Id': localStorage.getItem('tenantId') || 'default'
            }
          }
        )

        shippingOptions.value = response.data.options || []
        if (shippingOptions.value.length > 0 && !selectedShippingPrice.value) {
          selectedShippingPrice.value = shippingOptions.value[0].price
        }
      } catch (err) {
        console.error('Erro ao calcular frete:', err)
        shippingOptions.value = []
      } finally {
        calculatingShipping.value = false
      }
    }

    const updateShippingCost = () => {
      // O preço do frete será enviado junto com o pedido
    }

    const addItem = () => {
      form.value.items.push({
        skuId: null,
        productId: null,
        colorId: null,
        sizeId: null,
        productName: '',
        quantity: 1,
        unitPrice: 0
      })
    }

    const removeItem = (index) => {
      form.value.items.splice(index, 1)
    }

    const createOrder = async () => {
      loading.value = true
      error.value = ''
      success.value = ''

      try {
        const orderData = {
          ...form.value,
          shippingCost: selectedShippingPrice.value || 0
        }

        const token = localStorage.getItem('token')
        const response = await axios.post(
          `${API_BASE_URL}/orders`,
          orderData,
          {
            headers: {
              Authorization: `Bearer ${token}`,
              'X-Tenant-Id': localStorage.getItem('tenantId') || 'default'
            }
          }
        )

        success.value = response.data.id
        setTimeout(() => {
          router.push('/')
        }, 2000)
      } catch (err) {
        error.value = err.response?.data?.message || err.message || 'Erro ao criar pedido'
      } finally {
        loading.value = false
      }
    }

    // Recalcular frete quando itens mudarem
    watch(() => form.value.items, () => {
      if (form.value.shippingAddress.zipCode?.replace(/\D/g, '').length === 8) {
        calculateShipping()
      }
    }, { deep: true })

    onMounted(async () => {
      loadingSkus.value = true
      try {
        await Promise.all([
          stockStore.fetchSkusWithStock(),
          stockStore.fetchProducts(),
          stockStore.fetchColors(),
          stockStore.fetchSizes(),
          pricesStore.fetchPriceTables(true) // Carregar apenas tabelas ativas
        ])
        productsWithSkus.value = getProductsWithSkus()
        activePriceTables.value = pricesStore.priceTables.filter(pt => pt.isActive)
        
        // Selecionar primeira tabela ativa por padrão
        if (activePriceTables.value.length > 0 && !selectedPriceTableId.value) {
          selectedPriceTableId.value = activePriceTables.value[0].id
        }
      } catch (err) {
        error.value = 'Erro ao carregar produtos disponíveis: ' + (err.response?.data?.message || err.message)
      } finally {
        loadingSkus.value = false
      }
    })

    return {
      form,
      loading,
      loadingSkus,
      error,
      success,
      stockStore,
      pricesStore,
      productsWithSkus,
      shippingOptions,
      selectedShippingPrice,
      calculatingShipping,
      consultandoCep,
      selectedPriceTableId,
      activePriceTables,
      addItem,
      removeItem,
      createOrder,
      getMaxQuantity,
      onProductChange,
      onColorChange,
      onSizeChange,
      getAvailableColors,
      getAvailableSizes,
      validateQuantity,
      onCepInput,
      onCepBlur,
      calculateShipping,
      updateShippingCost,
      onPriceTableChange
    }
  }
}
</script>

<style scoped>
.create-order {
  padding: 2rem 0;
}

.order-form {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.form-group {
  margin-bottom: 1.5rem;
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
  margin-bottom: 0.5rem;
}

.address-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}

.cep-input-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.cep-input-group label {
  font-weight: 500;
  margin-bottom: 0.25rem;
}

.cep-input-group input {
  width: 100%;
}

.shipping-options {
  margin-top: 1rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 4px;
}

.shipping-options h4 {
  margin-bottom: 0.5rem;
  font-size: 1rem;
}

.shipping-option {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.shipping-option input[type="radio"] {
  width: auto;
}

.shipping-option label {
  cursor: pointer;
  font-weight: normal;
}

.items-section {
  margin: 2rem 0;
  padding: 1.5rem;
  background: #f8f9fa;
  border-radius: 8px;
}

.items-header {
  display: grid;
  grid-template-columns: 200px 120px 120px 100px 100px 120px auto;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
  padding: 0.5rem;
  background: #f3f4f6;
  border-radius: 4px;
  font-weight: 600;
  font-size: 0.875rem;
}

.header-cell {
  text-align: left;
}

.item-row {
  display: grid;
  grid-template-columns: 200px 120px 120px 100px 100px 120px auto;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
  align-items: center;
}

.price-table-selector {
  margin-bottom: 1rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 4px;
}

.price-table-selector label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.price-table-selector select {
  width: 100%;
  max-width: 400px;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.product-select {
  min-width: 200px;
}

.sku-info {
  display: flex;
  flex-direction: column;
  font-size: 0.875rem;
}

.stock-info {
  font-size: 0.875rem;
  color: #059669;
  font-weight: 500;
}

.loading {
  text-align: center;
  padding: 1rem;
  color: #666;
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
}

.btn-primary {
  background: #2563eb;
  color: white;
}

.btn-secondary {
  background: #6b7280;
  color: white;
}

.btn-danger {
  background: #dc2626;
  color: white;
}

.btn-sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
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

