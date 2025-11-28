import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || '')
  const username = ref('')
  const tenantId = ref('')

  const isAuthenticated = ref(!!token.value)

  if (token.value) {
    axios.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
  }

  const login = async (loginData) => {
    try {
      const response = await axios.post('http://localhost:5000/api/auth/login', loginData)
      token.value = response.data.token
      username.value = response.data.username
      tenantId.value = response.data.tenantId
      isAuthenticated.value = true
      
      localStorage.setItem('token', token.value)
      axios.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
    } catch (error) {
      console.error('Login failed:', error)
      throw error
    }
  }

  const logout = () => {
    token.value = ''
    username.value = ''
    tenantId.value = ''
    isAuthenticated.value = false
    localStorage.removeItem('token')
    delete axios.defaults.headers.common['Authorization']
  }

  return {
    token,
    username,
    tenantId,
    isAuthenticated,
    login,
    logout
  }
})


