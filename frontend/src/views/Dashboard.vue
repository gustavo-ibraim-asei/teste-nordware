<template>
  <div class="dashboard">
    <!-- Stats Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-primary">
        <div class="stat-icon">📦</div>
        <div class="stat-content">
          <h3>{{ totalOrders }}</h3>
          <p>Total de Pedidos</p>
        </div>
      </div>
      <div class="stat-card stat-success">
        <div class="stat-icon">✅</div>
        <div class="stat-content">
          <h3>{{ confirmedOrders }}</h3>
          <p>Confirmados</p>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon">⏳</div>
        <div class="stat-content">
          <h3>{{ pendingOrders }}</h3>
          <p>Pendentes</p>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon">💰</div>
        <div class="stat-content">
          <h3>R$ {{ totalRevenue.toFixed(2) }}</h3>
          <p>Receita Total</p>
        </div>
      </div>
    </div>

    <!-- Filters Section -->
    <div class="dashboard-section">
      <div class="section-header">
        <h2>Pedidos Recentes</h2>
        <div class="filters">
          <select v-model="filters.status" @change="loadOrders" class="filter-select">
            <option value="">Todos os status</option>
            <option value="1">Pendente</option>
            <option value="2">Confirmado</option>
            <option value="3">Processando</option>
            <option value="4">Enviado</option>
            <option value="5">Entregue</option>
            <option value="6">Cancelado</option>
          </select>
          <input 
            v-model.number="filters.customerId" 
            type="number" 
            placeholder="ID do Cliente"
            class="filter-input"
            @input="loadOrders"
          />
          <button @click="loadOrders" class="btn btn-primary btn-icon">
            <span>🔄</span>
            <span>Atualizar</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Carregando pedidos...</p>
    </div>

    <!-- Orders Grid -->
    <div v-else class="orders-grid">
      <div v-for="order in orders" :key="order.id" class="order-card">
        <div class="order-card-header">
          <div class="order-id">
            <span class="order-label">Pedido</span>
            <span class="order-number">#{{ order.id }}</span>
          </div>
          <span :class="['status-badge', getStatusClass(order.status)]">
            {{ getStatusName(order.status) }}
          </span>
        </div>
        
        <div class="order-card-body">
          <div class="order-info-row">
            <span class="info-label">Cliente ID:</span>
            <span class="info-value">{{ order.customerId }}</span>
          </div>
          <div class="order-info-row">
            <span class="info-label">Total:</span>
            <span class="info-value highlight">R$ {{ order.totalAmount.toFixed(2) }}</span>
          </div>
          <div class="order-info-row">
            <span class="info-label">Frete:</span>
            <span class="info-value">R$ {{ order.shippingCost.toFixed(2) }}</span>
          </div>
          <div class="order-info-row">
            <span class="info-label">Data:</span>
            <span class="info-value">{{ formatDate(order.createdAt) }}</span>
          </div>
        </div>

        <div class="order-items-section">
          <div class="items-header">
            <span class="items-count">{{ order.items.length }} {{ order.items.length === 1 ? 'item' : 'itens' }}</span>
          </div>
          <div class="items-list">
            <div v-for="item in order.items" :key="item.id" class="item-row">
              <span class="item-name">{{ item.productName }}</span>
              <span class="item-details">Qtd: {{ item.quantity }} × R$ {{ item.unitPrice.toFixed(2) }}</span>
              <span class="item-total">R$ {{ item.subtotal.toFixed(2) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="!loading && orders.length === 0" class="empty-state">
      <div class="empty-icon">📭</div>
      <h3>Nenhum pedido encontrado</h3>
      <p>Quando houver pedidos, eles aparecerão aqui</p>
      <router-link to="/create-order" class="btn btn-primary btn-large">
        Criar Primeiro Pedido
      </router-link>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useOrdersStore } from '../stores/orders'
import { useAuthStore } from '../stores/auth'

export default {
  name: 'Dashboard',
  setup() {
    const router = useRouter()
    const ordersStore = useOrdersStore()
    const authStore = useAuthStore()
    const loading = ref(false)
    const filters = ref({
      status: '',
      customerId: ''
    })

    const orders = computed(() => ordersStore.orders || [])

    const totalOrders = computed(() => orders.value.length)
    
    const confirmedOrders = computed(() => 
      orders.value.filter(o => o.status === 2).length
    )
    
    const pendingOrders = computed(() => 
      orders.value.filter(o => o.status === 1).length
    )
    
    const totalRevenue = computed(() => 
      orders.value
        .filter(o => o.status !== 6) // Excluir cancelados
        .reduce((sum, o) => sum + o.totalAmount, 0)
    )

    const loadOrders = async () => {
      loading.value = true
      try {
        await ordersStore.fetchOrders({
          ...filters.value,
          page: 1,
          pageSize: 50
        })
      } finally {
        loading.value = false
      }
    }

    const getStatusName = (status) => {
      const statusMap = {
        1: 'Pendente',
        2: 'Confirmado',
        3: 'Processando',
        4: 'Enviado',
        5: 'Entregue',
        6: 'Cancelado'
      }
      return statusMap[status] || 'Desconhecido'
    }

    const getStatusClass = (status) => {
      const classMap = {
        1: 'status-pending',
        2: 'status-confirmed',
        3: 'status-processing',
        4: 'status-shipped',
        5: 'status-delivered',
        6: 'status-cancelled'
      }
      return classMap[status] || ''
    }

    const formatDate = (dateString) => {
      const date = new Date(dateString)
      return date.toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    }

    onMounted(() => {
      loadOrders()
      if (authStore.tenantId) {
        ordersStore.connectSignalR(authStore.tenantId)
      }
    })

    onUnmounted(() => {
      ordersStore.disconnectSignalR()
    })

    return {
      orders,
      loading,
      filters,
      totalOrders,
      confirmedOrders,
      pendingOrders,
      totalRevenue,
      loadOrders,
      getStatusName,
      getStatusClass,
      formatDate
    }
  }
}
</script>

<style scoped>
.dashboard {
  padding: 0;
}

/* Stats Grid */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: linear-gradient(135deg, var(--bg-primary) 0%, var(--bg-secondary) 100%);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  box-shadow: var(--shadow-md);
  transition: all 0.3s;
  border: 1px solid var(--border-color);
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-xl);
}

.stat-icon {
  font-size: 3rem;
  line-height: 1;
}

.stat-content h3 {
  font-size: 2rem;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0;
  line-height: 1.2;
}

.stat-content p {
  font-size: 0.875rem;
  color: var(--text-secondary);
  margin: 0.25rem 0 0 0;
}

.stat-primary {
  border-left: 4px solid var(--primary-color);
}

.stat-success {
  border-left: 4px solid var(--secondary-color);
}

.stat-warning {
  border-left: 4px solid var(--warning-color);
}

.stat-info {
  border-left: 4px solid #3b82f6;
}

/* Dashboard Section */
.dashboard-section {
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  margin-bottom: 2rem;
  box-shadow: var(--shadow-sm);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
}

.section-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.filters {
  display: flex;
  gap: 0.75rem;
  align-items: center;
  flex-wrap: wrap;
}

.filter-select,
.filter-input {
  padding: 0.625rem 1rem;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  font-size: 0.9375rem;
  background: var(--bg-primary);
  color: var(--text-primary);
  transition: all 0.2s;
}

.filter-select:focus,
.filter-input:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.filter-input {
  width: 150px;
}

.btn-icon {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

/* Loading */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  gap: 1rem;
}

.spinner {
  width: 48px;
  height: 48px;
  border: 4px solid var(--border-color);
  border-top-color: var(--primary-color);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Orders Grid */
.orders-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 1.5rem;
}

.order-card {
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

.order-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-xl);
  border-color: var(--primary-color);
}

.order-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 1rem;
  border-bottom: 2px solid var(--bg-tertiary);
}

.order-id {
  display: flex;
  flex-direction: column;
}

.order-label {
  font-size: 0.75rem;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.order-number {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--text-primary);
}

.status-badge {
  padding: 0.375rem 0.875rem;
  border-radius: 9999px;
  font-size: 0.8125rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-pending { 
  background: #fef3c7; 
  color: #92400e; 
}

.status-confirmed { 
  background: #dbeafe; 
  color: #1e40af; 
}

.status-processing { 
  background: #e0e7ff; 
  color: #3730a3; 
}

.status-shipped { 
  background: #ddd6fe; 
  color: #5b21b6; 
}

.status-delivered { 
  background: #d1fae5; 
  color: #065f46; 
}

.status-cancelled { 
  background: #fee2e2; 
  color: #991b1b; 
}

.order-card-body {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.order-info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.9375rem;
}

.info-label {
  color: var(--text-secondary);
  font-weight: 500;
}

.info-value {
  color: var(--text-primary);
  font-weight: 600;
}

.info-value.highlight {
  color: var(--primary-color);
  font-size: 1.125rem;
}

.order-items-section {
  margin-top: 0.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--bg-tertiary);
}

.items-header {
  margin-bottom: 0.75rem;
}

.items-count {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.items-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.item-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem;
  background: var(--bg-secondary);
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
}

.item-name {
  flex: 1;
  font-weight: 500;
  color: var(--text-primary);
}

.item-details {
  color: var(--text-secondary);
  margin: 0 0.5rem;
}

.item-total {
  font-weight: 600;
  color: var(--primary-color);
}

/* Empty State */
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
  .stats-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }

  .stat-card {
    padding: 1.25rem;
  }

  .stat-icon {
    font-size: 2.5rem;
  }

  .stat-content h3 {
    font-size: 1.75rem;
  }

  .section-header {
    flex-direction: column;
    align-items: stretch;
  }

  .filters {
    width: 100%;
  }

  .filter-input {
    width: 100%;
  }

  .orders-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }

  .order-card {
    padding: 1.25rem;
  }

  .item-row {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.25rem;
  }

  .item-details {
    margin: 0;
  }
}

@media (max-width: 480px) {
  .dashboard {
    padding: 0;
  }

  .stats-grid {
    margin-bottom: 1rem;
  }

  .dashboard-section {
    padding: 1rem;
    margin-bottom: 1rem;
  }

  .section-header h2 {
    font-size: 1.25rem;
  }

  .filters {
    flex-direction: column;
  }

  .filter-select,
  .filter-input {
    width: 100%;
  }

  .btn-icon {
    width: 100%;
    justify-content: center;
  }
}
</style>
