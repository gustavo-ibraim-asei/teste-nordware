import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import { API_BASE_URL } from '../config/api'

const API_BASE = API_BASE_URL

export const usePricesStore = defineStore('prices', () => {
  const priceTables = ref([])
  const productPrices = ref([])
  const loading = ref(false)

  // Price Tables
  const fetchPriceTables = async (onlyActive = null) => {
    loading.value = true
    try {
      const params = onlyActive !== null ? `?onlyActive=${onlyActive}` : ''
      const response = await axios.get(`${API_BASE}/pricetables${params}`, {
        headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
      })
      priceTables.value = response.data
    } catch (error) {
      console.error('Error fetching price tables:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  const createPriceTable = async (data) => {
    const response = await axios.post(`${API_BASE}/pricetables`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchPriceTables()
    return response.data
  }

  const updatePriceTable = async (id, data) => {
    const response = await axios.put(`${API_BASE}/pricetables/${id}`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchPriceTables()
    return response.data
  }

  const deletePriceTable = async (id) => {
    await axios.delete(`${API_BASE}/pricetables/${id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchPriceTables()
  }

  // Product Prices
  const fetchProductPrices = async (productId = null, priceTableId = null) => {
    loading.value = true
    try {
      const params = new URLSearchParams()
      if (productId) params.append('productId', productId)
      if (priceTableId) params.append('priceTableId', priceTableId)
      const url = params.toString() 
        ? `${API_BASE}/productprices?${params}`
        : `${API_BASE}/productprices`
      const response = await axios.get(url, {
        headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
      })
      productPrices.value = response.data
    } catch (error) {
      console.error('Error fetching product prices:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  const createProductPrice = async (data) => {
    const response = await axios.post(`${API_BASE}/productprices`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchProductPrices()
    return response.data
  }

  const updateProductPrice = async (id, data) => {
    const response = await axios.put(`${API_BASE}/productprices/${id}`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchProductPrices()
    return response.data
  }

  const deleteProductPrice = async (id) => {
    await axios.delete(`${API_BASE}/productprices/${id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchProductPrices()
  }

  const getProductPrice = async (productId, priceTableId) => {
    try {
      const response = await axios.get(
        `${API_BASE}/productprices/product/${productId}/pricetable/${priceTableId}`,
        {
          headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
        }
      )
      return response.data
    } catch (error) {
      if (error.response?.status === 404) {
        return null // Preço não encontrado
      }
      console.error('Error fetching product price:', error)
      throw error
    }
  }

  return {
    priceTables,
    productPrices,
    loading,
    fetchPriceTables,
    createPriceTable,
    updatePriceTable,
    deletePriceTable,
    fetchProductPrices,
    createProductPrice,
    updateProductPrice,
    deleteProductPrice,
    getProductPrice
  }
})

