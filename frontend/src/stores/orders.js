import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import * as signalR from '@microsoft/signalr'

export const useOrdersStore = defineStore('orders', () => {
  const orders = ref([])
  const loading = ref(false)
  const connection = ref(null)

  const connectSignalR = (tenantId) => {
    if (connection.value) {
      connection.value.stop()
    }

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5000/orderHub')
      .withAutomaticReconnect()
      .build()

    connection.value.start()
      .then(() => {
        console.log('SignalR connected')
        connection.value.invoke('JoinTenantGroup', tenantId)
      })
      .catch(err => console.error('SignalR connection error:', err))

    connection.value.on('OrderCreated', (order) => {
      orders.value.unshift(order)
    })

    connection.value.on('OrderStatusUpdated', (order) => {
      const index = orders.value.findIndex(o => o.id === order.id)
      if (index !== -1) {
        orders.value[index] = order
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

      const response = await axios.get(`http://localhost:5000/api/orders?${params}`)
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


