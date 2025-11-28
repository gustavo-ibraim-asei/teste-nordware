<template>
  <div class="dashboard">
    <div class="dashboard-header">
      <h2>Dashboard de Pedidos</h2>
      <div class="filters">
        <select v-model="filters.status" @change="loadOrders">
          <option value="">Todos os status</option>
          <option value="1">Pending</option>
          <option value="2">Confirmed</option>
          <option value="3">Processing</option>
          <option value="4">Shipped</option>
          <option value="5">Delivered</option>
          <option value="6">Cancelled</option>
        </select>
        <input 
          v-model="filters.customerId" 
          type="number" 
          placeholder="Customer ID"
          @input="loadOrders"
        />
        <button @click="loadOrders" class="btn btn-primary">Atualizar</button>
      </div>
    </div>

    <div v-if="loading" class="loading">Carregando...</div>

    <div v-else class="orders-grid">
      <div v-for="order in orders" :key="order.id" class="order-card">
        <div class="order-header">
          <h3>Pedido #{{ order.id }}</h3>
          <span :class="['status-badge', getStatusClass(order.status)]">
            {{ getStatusName(order.status) }}
          </span>
        </div>
        <div class="order-info">
          <p><strong>Cliente:</strong> {{ order.customerId }}</p>
          <p><strong>Total:</strong> R$ {{ order.totalAmount.toFixed(2) }}</p>
          <p><strong>Frete:</strong> R$ {{ order.shippingCost.toFixed(2) }}</p>
          <p><strong>Criado em:</strong> {{ formatDate(order.createdAt) }}</p>
        </div>
        <div class="order-items">
          <h4>Itens ({{ order.items.length }})</h4>
          <ul>
            <li v-for="item in order.items" :key="item.id">
              {{ item.productName }} - Qtd: {{ item.quantity }} - R$ {{ item.subtotal.toFixed(2) }}
            </li>
          </ul>
        </div>
      </div>
    </div>

    <div v-if="!loading && orders.length === 0" class="empty-state">
      <p>Nenhum pedido encontrado</p>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, onUnmounted } from 'vue'
import { useOrdersStore } from '../stores/orders'
import { useAuthStore } from '../stores/auth'

export default {
  name: 'Dashboard',
  setup() {
    const ordersStore = useOrdersStore()
    const authStore = useAuthStore()
    const loading = ref(false)
    const filters = ref({
      status: '',
      customerId: ''
    })

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
        1: 'Pending',
        2: 'Confirmed',
        3: 'Processing',
        4: 'Shipped',
        5: 'Delivered',
        6: 'Cancelled'
      }
      return statusMap[status] || 'Unknown'
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
      return new Date(dateString).toLocaleString('pt-BR')
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
      orders: ordersStore.orders,
      loading,
      filters,
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
  padding: 2rem 0;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  flex-wrap: wrap;
  gap: 1rem;
}

.filters {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.filters select,
.filters input {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.orders-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.order-card {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  transition: transform 0.2s;
}

.order-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.15);
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #eee;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.875rem;
  font-weight: 500;
}

.status-pending { background: #fef3c7; color: #92400e; }
.status-confirmed { background: #dbeafe; color: #1e40af; }
.status-processing { background: #e0e7ff; color: #3730a3; }
.status-shipped { background: #ddd6fe; color: #5b21b6; }
.status-delivered { background: #d1fae5; color: #065f46; }
.status-cancelled { background: #fee2e2; color: #991b1b; }

.order-info {
  margin-bottom: 1rem;
}

.order-info p {
  margin-bottom: 0.5rem;
  color: #666;
}

.order-items {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #eee;
}

.order-items h4 {
  margin-bottom: 0.5rem;
  font-size: 0.875rem;
  color: #333;
}

.order-items ul {
  list-style: none;
  font-size: 0.875rem;
}

.order-items li {
  padding: 0.25rem 0;
  color: #666;
}

.loading,
.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: #666;
}
</style>


