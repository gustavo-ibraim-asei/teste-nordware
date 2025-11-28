import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import { API_BASE_URL } from '../config/api'

const API_BASE = API_BASE_URL

export const useCustomersStore = defineStore('customers', () => {
  const customers = ref([])
  const loading = ref(false)

  const fetchCustomers = async () => {
    loading.value = true
    try {
      const response = await axios.get(`${API_BASE}/customers`, {
        headers: { 
          Authorization: `Bearer ${localStorage.getItem('token')}`,
          'X-Tenant-Id': localStorage.getItem('tenantId') || 'default'
        }
      })
      customers.value = response.data
    } catch (error) {
      console.error('Error fetching customers:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  const createCustomer = async (data) => {
    const response = await axios.post(`${API_BASE}/customers`, data, {
      headers: { 
        Authorization: `Bearer ${localStorage.getItem('token')}`,
        'X-Tenant-Id': localStorage.getItem('tenantId') || 'default'
      }
    })
    await fetchCustomers()
    return response.data
  }

  const updateCustomer = async (id, data) => {
    const response = await axios.put(`${API_BASE}/customers/${id}`, data, {
      headers: { 
        Authorization: `Bearer ${localStorage.getItem('token')}`,
        'X-Tenant-Id': localStorage.getItem('tenantId') || 'default'
      }
    })
    await fetchCustomers()
    return response.data
  }

  const deleteCustomer = async (id) => {
    await axios.delete(`${API_BASE}/customers/${id}`, {
      headers: { 
        Authorization: `Bearer ${localStorage.getItem('token')}`,
        'X-Tenant-Id': localStorage.getItem('tenantId') || 'default'
      }
    })
    await fetchCustomers()
  }

  const getCustomerById = async (id) => {
    loading.value = true
    try {
      const response = await axios.get(`${API_BASE}/customers/${id}`, {
        headers: { 
          Authorization: `Bearer ${localStorage.getItem('token')}`,
          'X-Tenant-Id': localStorage.getItem('tenantId') || 'default'
        }
      })
      return response.data
    } catch (error) {
      console.error('Error fetching customer:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  return {
    customers,
    loading,
    fetchCustomers,
    createCustomer,
    updateCustomer,
    deleteCustomer,
    getCustomerById
  }
})

