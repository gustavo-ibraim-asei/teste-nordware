import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import { API_BASE_URL } from '../config/api'

const API_BASE = API_BASE_URL

export const useStockStore = defineStore('stock', () => {
  const stockOffices = ref([])
  const colors = ref([])
  const sizes = ref([])
  const skus = ref([])
  const stocks = ref([])
  const loading = ref(false)

  // Stock Offices
  const fetchStockOffices = async () => {
    loading.value = true
    try {
      const response = await axios.get(`${API_BASE}/stockoffices`, {
        headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
      })
      stockOffices.value = response.data
    } catch (error) {
      console.error('Error fetching stock offices:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  const createStockOffice = async (data) => {
    const response = await axios.post(`${API_BASE}/stockoffices`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchStockOffices()
    return response.data
  }

  const updateStockOffice = async (id, data) => {
    const response = await axios.put(`${API_BASE}/stockoffices/${id}`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchStockOffices()
    return response.data
  }

  const deleteStockOffice = async (id) => {
    await axios.delete(`${API_BASE}/stockoffices/${id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchStockOffices()
  }

  // Colors
  const fetchColors = async () => {
    loading.value = true
    try {
      const response = await axios.get(`${API_BASE}/colors`, {
        headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
      })
      colors.value = response.data
    } catch (error) {
      console.error('Error fetching colors:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  const createColor = async (data) => {
    const response = await axios.post(`${API_BASE}/colors`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchColors()
    return response.data
  }

  const updateColor = async (id, data) => {
    const response = await axios.put(`${API_BASE}/colors/${id}`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchColors()
    return response.data
  }

  const deleteColor = async (id) => {
    await axios.delete(`${API_BASE}/colors/${id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchColors()
  }

  // Sizes
  const fetchSizes = async () => {
    loading.value = true
    try {
      const response = await axios.get(`${API_BASE}/sizes`, {
        headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
      })
      sizes.value = response.data
    } catch (error) {
      console.error('Error fetching sizes:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  const createSize = async (data) => {
    const response = await axios.post(`${API_BASE}/sizes`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchSizes()
    return response.data
  }

  const updateSize = async (id, data) => {
    const response = await axios.put(`${API_BASE}/sizes/${id}`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchSizes()
    return response.data
  }

  const deleteSize = async (id) => {
    await axios.delete(`${API_BASE}/sizes/${id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchSizes()
  }

  // SKUs
  const fetchSkus = async (productId = null) => {
    loading.value = true
    try {
      const url = productId 
        ? `${API_BASE}/skus?productId=${productId}`
        : `${API_BASE}/skus`
      const response = await axios.get(url, {
        headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
      })
      skus.value = response.data
    } catch (error) {
      console.error('Error fetching SKUs:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  const createSku = async (data) => {
    const response = await axios.post(`${API_BASE}/skus`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchSkus()
    return response.data
  }

  const updateSku = async (id, data) => {
    const response = await axios.put(`${API_BASE}/skus/${id}`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchSkus()
    return response.data
  }

  const deleteSku = async (id) => {
    await axios.delete(`${API_BASE}/skus/${id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchSkus()
  }

  // Stocks
  const fetchStocks = async (skuId = null, stockOfficeId = null) => {
    loading.value = true
    try {
      const params = new URLSearchParams()
      if (skuId) params.append('skuId', skuId)
      if (stockOfficeId) params.append('stockOfficeId', stockOfficeId)
      const url = params.toString() 
        ? `${API_BASE}/stocks?${params}`
        : `${API_BASE}/stocks`
      const response = await axios.get(url, {
        headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
      })
      stocks.value = response.data
    } catch (error) {
      console.error('Error fetching stocks:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  const createStock = async (data) => {
    const response = await axios.post(`${API_BASE}/stocks`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchStocks()
    return response.data
  }

  const updateStock = async (id, data) => {
    const response = await axios.put(`${API_BASE}/stocks/${id}`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchStocks()
    return response.data
  }

  const deleteStock = async (id) => {
    await axios.delete(`${API_BASE}/stocks/${id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchStocks()
  }

  // Products
  const products = ref([])

  const fetchProducts = async () => {
    loading.value = true
    try {
      const response = await axios.get(`${API_BASE}/products`, {
        headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
      })
      products.value = response.data
    } catch (error) {
      console.error('Error fetching products:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  const createProduct = async (data) => {
    const response = await axios.post(`${API_BASE}/products`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchProducts()
    return response.data
  }

  const updateProduct = async (id, data) => {
    const response = await axios.put(`${API_BASE}/products/${id}`, data, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchProducts()
    return response.data
  }

  const deleteProduct = async (id) => {
    await axios.delete(`${API_BASE}/products/${id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    await fetchProducts()
  }

  // SKUs with stock
  const skusWithStock = ref([])

  const fetchSkusWithStock = async () => {
    loading.value = true
    try {
      const response = await axios.get(`${API_BASE}/skus/with-stock`, {
        headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
      })
      skusWithStock.value = response.data
    } catch (error) {
      console.error('Error fetching SKUs with stock:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  return {
    stockOffices,
    colors,
    sizes,
    products,
    skus,
    stocks,
    loading,
    fetchStockOffices,
    createStockOffice,
    updateStockOffice,
    deleteStockOffice,
    fetchColors,
    createColor,
    updateColor,
    deleteColor,
    fetchSizes,
    createSize,
    updateSize,
    deleteSize,
    fetchProducts,
    createProduct,
    updateProduct,
    deleteProduct,
    fetchSkus,
    createSku,
    updateSku,
    deleteSku,
    fetchStocks,
    createStock,
    updateStock,
    deleteStock,
    skusWithStock,
    fetchSkusWithStock
  }
})

