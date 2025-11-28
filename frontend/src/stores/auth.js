import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import { API_BASE_URL } from '../config/api'

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
      // A API espera EmailOrUserName, não username
      const requestData = {
        emailOrUserName: loginData.username || loginData.emailOrUserName,
        password: loginData.password,
        tenantId: loginData.tenantId || null
      }
      
      const response = await axios.post(`${API_BASE_URL}/auth/login`, requestData)
      
      // A resposta retorna Token, Username, Email, TenantId
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

  const register = async (registerData) => {
    try {
      // A API espera Email, UserName, Password, TenantId
      const requestData = {
        email: registerData.email,
        userName: registerData.userName,
        password: registerData.password,
        tenantId: registerData.tenantId
      }
      
      const response = await axios.post(`${API_BASE_URL}/auth/register`, requestData)
      // Registro bem-sucedido, mas não faz login automaticamente
      return response.data
    } catch (error) {
      console.error('Registration failed:', error)
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
    register,
    logout
  }
})





