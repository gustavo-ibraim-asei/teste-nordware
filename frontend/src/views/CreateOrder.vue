<template>
  <div class="create-order-page">
    <div class="page-header">
      <div class="header-content">
        <h1>🛒 Criar Novo Pedido</h1>
        <p>Preencha os dados do pedido abaixo</p>
      </div>
    </div>

    <form @submit.prevent="createOrder" class="order-form">
      <!-- Customer Selection -->
      <div class="form-section">
        <div class="section-title">
          <span class="section-icon">👤</span>
          <h2>Dados do Cliente</h2>
        </div>
        <div class="form-group">
          <label>Cliente *</label>
          <select 
            v-model.number="form.customerId" 
            required 
            class="form-select"
            :disabled="loadingCustomers"
          >
            <option value="">Selecione um cliente</option>
            <option 
              v-for="customer in customers" 
              :key="customer.id" 
              :value="customer.id"
            >
              {{ customer.name }} - {{ customer.email }}
            </option>
          </select>
          <small v-if="!form.customerId" class="form-hint">
            Selecione o cliente que realizará o pedido
          </small>
        </div>
      </div>

      <!-- Shipping Address -->
      <div class="form-section">
        <div class="section-title">
          <span class="section-icon">📍</span>
          <h2>Endereço de Entrega</h2>
        </div>
        <div class="address-grid">
          <div class="form-group cep-group">
            <label>CEP *</label>
            <div class="input-with-loader">
              <input 
                v-model="form.shippingAddress.zipCode" 
                placeholder="00000-000" 
                required 
                @blur="onCepBlur"
                @input="onCepInput"
                :disabled="consultandoCep"
                maxlength="9"
                class="form-input"
              />
              <span v-if="consultandoCep" class="input-loader">⏳</span>
            </div>
            <small class="form-hint">Digite o CEP para preencher automaticamente</small>
          </div>
          <div class="form-group">
            <label>Rua *</label>
            <input 
              v-model="form.shippingAddress.street" 
              placeholder="Nome da rua" 
              required 
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label>Número *</label>
            <input 
              v-model="form.shippingAddress.number" 
              placeholder="123" 
              required 
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label>Complemento</label>
            <input 
              v-model="form.shippingAddress.complement" 
              placeholder="Apto, Bloco, etc." 
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label>Bairro *</label>
            <input 
              v-model="form.shippingAddress.neighborhood" 
              placeholder="Nome do bairro" 
              required 
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label>Cidade *</label>
            <input 
              v-model="form.shippingAddress.city" 
              placeholder="Nome da cidade" 
              required 
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label>Estado *</label>
            <input 
              v-model="form.shippingAddress.state" 
              placeholder="UF" 
              required 
              maxlength="2"
              class="form-input"
            />
          </div>
        </div>

        <!-- Shipping Options -->
        <div v-if="shippingOptions.length > 0" class="shipping-section">
          <h3>Opções de Frete</h3>
          <div class="shipping-options-grid">
            <label 
              v-for="option in shippingOptions" 
              :key="option.carrierId + '-' + option.shippingTypeId" 
              class="shipping-option-card"
              :class="{ 'selected': selectedShippingPrice === option.price }"
            >
              <input 
                type="radio" 
                :id="`shipping-${option.carrierId}-${option.shippingTypeId}`"
                :value="option.price" 
                v-model="selectedShippingPrice"
                @change="updateShippingCost"
                class="shipping-radio"
              />
              <div class="shipping-option-content">
                <div class="shipping-name">{{ option.description }}</div>
                <div class="shipping-details">
                  <span class="shipping-price">R$ {{ option.price.toFixed(2) }}</span>
                  <span class="shipping-days">⏱️ {{ option.estimatedDays }} dias</span>
                </div>
              </div>
            </label>
          </div>
        </div>
        <div v-if="calculatingShipping" class="shipping-loading">
          <span>⏳</span>
          <span>Calculando frete...</span>
        </div>
      </div>

      <!-- Order Items -->
      <div class="form-section">
        <div class="section-title">
          <span class="section-icon">📦</span>
          <h2>Itens do Pedido</h2>
        </div>

        <div class="price-table-selector">
          <label>Tabela de Preços *</label>
          <select 
            v-model="selectedPriceTableId" 
            @change="onPriceTableChange" 
            required
            class="form-select"
            :disabled="loadingSkus"
          >
            <option value="">Selecione uma tabela de preços</option>
            <option 
              v-for="priceTable in activePriceTables" 
              :key="priceTable.id" 
              :value="priceTable.id"
            >
              {{ priceTable.name }}
            </option>
          </select>
        </div>

        <div v-if="loadingSkus" class="loading-container">
          <div class="spinner"></div>
          <p>Carregando produtos disponíveis...</p>
        </div>

        <div v-else class="items-container">
          <div 
            v-for="(item, index) in form.items" 
            :key="index" 
            class="item-card"
          >
            <div class="item-card-header">
              <span class="item-number">Item {{ index + 1 }}</span>
              <button 
                type="button" 
                @click="removeItem(index)" 
                class="btn-icon-danger"
                :disabled="form.items.length === 1"
                title="Remover item"
              >
                🗑️
              </button>
            </div>
            <div class="item-card-body">
              <div class="item-form-grid">
                <div class="form-group">
                  <label>Produto *</label>
                  <select 
                    v-model="item.productId" 
                    @change="onProductChange(index)" 
                    required 
                    class="form-select"
                  >
                    <option value="">Selecione o Produto</option>
                    <option 
                      v-for="product in productsWithSkus" 
                      :key="product.id" 
                      :value="product.id"
                    >
                      {{ product.name }} ({{ product.code }})
                    </option>
                  </select>
                </div>
                <div class="form-group">
                  <label>Cor *</label>
                  <select 
                    v-model="item.colorId" 
                    @change="onColorChange(index)" 
                    required 
                    :disabled="!item.productId"
                    class="form-select"
                  >
                    <option value="">Selecione a Cor</option>
                    <option 
                      v-for="color in getAvailableColors(item.productId)" 
                      :key="color.id" 
                      :value="color.id"
                    >
                      {{ color.name }}
                    </option>
                  </select>
                </div>
                <div class="form-group">
                  <label>Tamanho *</label>
                  <select 
                    v-model="item.sizeId" 
                    @change="onSizeChange(index)" 
                    required 
                    :disabled="!item.productId || !item.colorId"
                    class="form-select"
                  >
                    <option value="">Selecione o Tamanho</option>
                    <option 
                      v-for="size in getAvailableSizes(item.productId, item.colorId)" 
                      :key="size.id" 
                      :value="size.id"
                    >
                      {{ size.name }}
                    </option>
                  </select>
                </div>
                <div class="form-group" v-if="item.skuId">
                  <label>Estoque Disponível</label>
                  <div class="stock-badge">
                    <span class="stock-icon">📦</span>
                    <span class="stock-value">{{ getMaxQuantity(item.skuId) }} unidades</span>
                  </div>
                </div>
                <div class="form-group">
                  <label>Quantidade *</label>
                  <input 
                    v-model.number="item.quantity" 
                    type="number" 
                    placeholder="Qtd" 
                    required 
                    min="1"
                    :max="getMaxQuantity(item.skuId)"
                    @input="validateQuantity(index)"
                    :disabled="!item.skuId"
                    class="form-input"
                  />
                </div>
                <div class="form-group">
                  <label>Preço Unitário *</label>
                  <div class="price-input-group">
                    <span class="price-symbol">R$</span>
                    <input 
                      v-model.number="item.unitPrice" 
                      type="number" 
                      step="0.01" 
                      placeholder="0.00" 
                      required 
                      :disabled="!item.productId || !selectedPriceTableId"
                      class="form-input price-input"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
          <button 
            type="button" 
            @click="addItem" 
            class="btn btn-outline btn-add-item"
          >
            <span>➕</span>
            <span>Adicionar Item</span>
          </button>
        </div>
      </div>

      <!-- Order Summary -->
      <div class="order-summary">
        <div class="summary-header">
          <h3>Resumo do Pedido</h3>
        </div>
        <div class="summary-content">
          <div class="summary-row">
            <span>Subtotal:</span>
            <span>R$ {{ orderSubtotal.toFixed(2) }}</span>
          </div>
          <div class="summary-row">
            <span>Frete:</span>
            <span>R$ {{ (selectedShippingPrice || 0).toFixed(2) }}</span>
          </div>
          <div class="summary-row summary-total">
            <span>Total:</span>
            <span>R$ {{ orderTotal.toFixed(2) }}</span>
          </div>
        </div>
      </div>

      <!-- Form Actions -->
      <div class="form-actions">
        <button 
          type="submit" 
          :disabled="loading || !canSubmit" 
          class="btn btn-primary btn-large"
        >
          <span v-if="loading">⏳</span>
          <span v-else>✅</span>
          <span>{{ loading ? 'Criando Pedido...' : 'Criar Pedido' }}</span>
        </button>
        <button 
          type="button" 
          @click="$router.push('/')" 
          class="btn btn-outline btn-large"
        >
          Cancelar
        </button>
      </div>

      <!-- Messages -->
      <div v-if="error" class="alert alert-error">
        <span>⚠️</span>
        <span>{{ error }}</span>
      </div>
      <div v-if="success" class="alert alert-success">
        <span>✅</span>
        <span>Pedido criado com sucesso! ID: {{ success }}</span>
      </div>
    </form>
  </div>
</template>

<script>
import { ref, computed, onMounted, watch } from 'vue'
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
    const loadingCustomers = ref(false)
    const error = ref('')
    const success = ref('')
    const customers = ref([])

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

    const orderSubtotal = computed(() => {
      return form.value.items.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0)
    })

    const orderTotal = computed(() => {
      return orderSubtotal.value + (selectedShippingPrice.value || 0)
    })

    const canSubmit = computed(() => {
      return form.value.customerId && 
             form.value.items.every(item => item.skuId && item.quantity > 0 && item.unitPrice > 0) &&
             form.value.shippingAddress.zipCode?.replace(/\D/g, '').length === 8
    })

    const loadCustomers = async () => {
      loadingCustomers.value = true
      try {
        const token = localStorage.getItem('token')
        const response = await axios.get(`${API_BASE_URL}/customers`, {
          headers: {
            Authorization: `Bearer ${token}`,
            'X-Tenant-Id': localStorage.getItem('tenantId') || 'default'
          }
        })
        customers.value = response.data || []
      } catch (err) {
        console.error('Erro ao carregar clientes:', err)
        error.value = 'Erro ao carregar lista de clientes'
      } finally {
        loadingCustomers.value = false
      }
    }

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
          if (!item.unitPrice) {
            item.unitPrice = 0
          }
        }
      } catch (err) {
        console.error('Erro ao carregar preço:', err)
      }
    }

    const onPriceTableChange = async () => {
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
        if (err.response?.status !== 404) {
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
        const totalWeight = form.value.items.reduce((sum, item) => sum + (item.quantity * 0.5), 0)

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
      if (form.value.items.length > 1) {
        form.value.items.splice(index, 1)
      }
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

    watch(() => form.value.items, () => {
      if (form.value.shippingAddress.zipCode?.replace(/\D/g, '').length === 8) {
        calculateShipping()
      }
    }, { deep: true })

    onMounted(async () => {
      await loadCustomers()
      loadingSkus.value = true
      try {
        await Promise.all([
          stockStore.fetchSkusWithStock(),
          stockStore.fetchProducts(),
          stockStore.fetchColors(),
          stockStore.fetchSizes(),
          pricesStore.fetchPriceTables(true)
        ])
        productsWithSkus.value = getProductsWithSkus()
        activePriceTables.value = pricesStore.priceTables.filter(pt => pt.isActive)
        
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
      loadingCustomers,
      loadingSkus,
      error,
      success,
      customers,
      stockStore,
      pricesStore,
      productsWithSkus,
      shippingOptions,
      selectedShippingPrice,
      calculatingShipping,
      consultandoCep,
      selectedPriceTableId,
      activePriceTables,
      orderSubtotal,
      orderTotal,
      canSubmit,
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
.create-order-page {
  padding: 0;
}

.page-header {
  margin-bottom: 2rem;
  padding: 1.5rem;
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
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

.order-form {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.form-section {
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  box-shadow: var(--shadow-sm);
  border: 1px solid var(--border-color);
}

.section-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 2px solid var(--bg-tertiary);
}

.section-icon {
  font-size: 1.5rem;
}

.section-title h2 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: var(--text-primary);
  font-size: 0.9375rem;
}

.form-input,
.form-select {
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
.form-select:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.form-input:disabled,
.form-select:disabled {
  background: var(--bg-tertiary);
  cursor: not-allowed;
  opacity: 0.6;
}

.form-hint {
  display: block;
  margin-top: 0.25rem;
  font-size: 0.8125rem;
  color: var(--text-secondary);
}

/* Address Grid */
.address-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.cep-group {
  grid-column: 1 / -1;
}

.input-with-loader {
  position: relative;
}

.input-loader {
  position: absolute;
  right: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  font-size: 1.25rem;
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

/* Shipping Section */
.shipping-section {
  margin-top: 1.5rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--border-color);
}

.shipping-section h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 1rem;
}

.shipping-options-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 1rem;
}

.shipping-option-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  border: 2px solid var(--border-color);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all 0.2s;
  background: var(--bg-primary);
}

.shipping-option-card:hover {
  border-color: var(--primary-color);
  background: var(--bg-secondary);
}

.shipping-option-card.selected {
  border-color: var(--primary-color);
  background: rgba(37, 99, 235, 0.05);
}

.shipping-radio {
  width: 20px;
  height: 20px;
  cursor: pointer;
}

.shipping-option-content {
  flex: 1;
}

.shipping-name {
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 0.25rem;
}

.shipping-details {
  display: flex;
  gap: 1rem;
  font-size: 0.875rem;
}

.shipping-price {
  color: var(--primary-color);
  font-weight: 600;
}

.shipping-days {
  color: var(--text-secondary);
}

.shipping-loading {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem;
  color: var(--text-secondary);
  font-size: 0.9375rem;
}

/* Items Section */
.price-table-selector {
  margin-bottom: 1.5rem;
  padding: 1rem;
  background: var(--bg-secondary);
  border-radius: var(--radius-md);
}

.price-table-selector label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.items-container {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.item-card {
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  overflow: hidden;
  background: var(--bg-secondary);
}

.item-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1rem;
  background: var(--bg-tertiary);
  border-bottom: 1px solid var(--border-color);
}

.item-number {
  font-weight: 600;
  color: var(--text-primary);
}

.btn-icon-danger {
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: var(--radius-sm);
  transition: all 0.2s;
}

.btn-icon-danger:hover:not(:disabled) {
  background: #fee2e2;
  transform: scale(1.1);
}

.btn-icon-danger:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.item-card-body {
  padding: 1rem;
}

.item-form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.stock-badge {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem;
  background: rgba(16, 185, 129, 0.1);
  border-radius: var(--radius-sm);
  color: var(--secondary-color);
  font-weight: 600;
}

.stock-icon {
  font-size: 1.125rem;
}

.price-input-group {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.price-symbol {
  color: var(--text-secondary);
  font-weight: 600;
}

.price-input {
  flex: 1;
}

.btn-add-item {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  margin-top: 0.5rem;
}

/* Order Summary */
.order-summary {
  background: linear-gradient(135deg, var(--bg-primary) 0%, var(--bg-secondary) 100%);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  box-shadow: var(--shadow-md);
  border: 2px solid var(--primary-color);
}

.summary-header {
  margin-bottom: 1rem;
  padding-bottom: 1rem;
  border-bottom: 2px solid var(--border-color);
}

.summary-header h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.summary-content {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.summary-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.9375rem;
}

.summary-row span:first-child {
  color: var(--text-secondary);
}

.summary-row span:last-child {
  font-weight: 600;
  color: var(--text-primary);
}

.summary-total {
  margin-top: 0.5rem;
  padding-top: 0.75rem;
  border-top: 2px solid var(--border-color);
  font-size: 1.25rem;
}

.summary-total span:last-child {
  color: var(--primary-color);
  font-size: 1.5rem;
}

/* Form Actions */
.form-actions {
  display: flex;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--border-color);
}

/* Alerts */
.alert {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem;
  border-radius: var(--radius-md);
  font-size: 0.9375rem;
}

.alert-error {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.alert-success {
  background: #d1fae5;
  color: #065f46;
  border: 1px solid #a7f3d0;
}

/* Loading */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 2rem;
  gap: 1rem;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid var(--border-color);
  border-top-color: var(--primary-color);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Responsive */
@media (max-width: 768px) {
  .page-header {
    padding: 1rem;
  }

  .header-content h1 {
    font-size: 1.75rem;
  }

  .form-section {
    padding: 1rem;
  }

  .address-grid {
    grid-template-columns: 1fr;
  }

  .shipping-options-grid {
    grid-template-columns: 1fr;
  }

  .item-form-grid {
    grid-template-columns: 1fr;
  }

  .form-actions {
    flex-direction: column;
  }

  .btn-large {
    width: 100%;
  }
}

@media (max-width: 480px) {
  .order-form {
    gap: 1.5rem;
  }

  .form-section {
    padding: 0.75rem;
  }

  .section-title h2 {
    font-size: 1.125rem;
  }
}
</style>
