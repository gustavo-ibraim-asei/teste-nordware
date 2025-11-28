import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import * as signalR from '@microsoft/signalr'
import { API_BASE_URL, API_SIGNALR_URL } from '../config/api'

export const useOrdersStore = defineStore('orders', () => {
  const orders = ref([])
  const loading = ref(false)
  const connection = ref(null)

  const connectSignalR = (tenantId) => {
    if (connection.value) {
      connection.value.stop()
    }

    const token = localStorage.getItem('token')
    if (!token) {
      console.error('No token available for SignalR connection')
      return
    }

    const url = `${API_SIGNALR_URL}?access_token=${encodeURIComponent(token)}`
    console.log('Connecting to SignalR:', url)

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.LongPolling, // Usar LongPolling para evitar problemas com WebSocket
        logger: signalR.LogLevel.Information // Habilitar logs
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
          if (retryContext.elapsedMilliseconds < 60000) {
            return 2000
          }
          return null
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build()

    // Adicionar handlers de eventos de conexão
    connection.value.onclose((error) => {
      console.log('SignalR connection closed', error)
    })

    connection.value.onreconnecting((error) => {
      console.log('SignalR reconnecting', error)
    })

    connection.value.onreconnected((connectionId) => {
      console.log('SignalR reconnected', connectionId)
      connection.value.invoke('JoinTenantGroup', tenantId).catch(err => {
        console.error('Error rejoining tenant group:', err)
      })
    })

    connection.value.start()
      .then(() => {
        console.log('SignalR connected successfully')
        return connection.value.invoke('JoinTenantGroup', tenantId)
      })
      .then(() => {
        console.log('Joined tenant group:', tenantId)
      })
      .catch(err => {
        console.error('SignalR connection error:', err)
        console.error('Error details:', err.message)
        console.error('Error stack:', err.stack)
      })

    connection.value.on('OrderCreated', async (order) => {
      console.log('OrderCreated received via SignalR:', order)
      // Adicionar o novo pedido no início da lista
      orders.value.unshift(order)
      // Recarregar os pedidos para garantir que as estatísticas sejam atualizadas
      try {
        await fetchOrders({ page: 1, pageSize: 50 })
      } catch (error) {
        console.error('Error reloading orders after SignalR notification:', error)
      }
    })

    connection.value.on('OrderStatusUpdated', async (order) => {
      console.log('OrderStatusUpdated received via SignalR:', order)
      const index = orders.value.findIndex(o => o.id === order.id)
      if (index !== -1) {
        orders.value[index] = order
      } else {
        // Se o pedido não estiver na lista, recarregar
        try {
          await fetchOrders({ page: 1, pageSize: 50 })
        } catch (error) {
          console.error('Error reloading orders after SignalR notification:', error)
        }
      }
    })
  }

  const fetchOrders = async (filters = {}) => {
    loading.value = true
    try {
      const params = new URLSearchParams()
      if (filters.customerId) params.append('customerId', filters.customerId)
      if (filters.status) params.append('status', filters.status)
      if (filters.page) params.append('page', filters.page)
      if (filters.pageSize) params.append('pageSize', filters.pageSize)

      const response = await axios.get(`${API_BASE_URL}/orders?${params}`)
      orders.value = response.data.items
      return response.data
    } catch (error) {
      console.error('Error fetching orders:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  const disconnectSignalR = () => {
    if (connection.value) {
      connection.value.stop()
      connection.value = null
    }
  }

  return {
    orders,
    loading,
    fetchOrders,
    connectSignalR,
    disconnectSignalR
  }
})





