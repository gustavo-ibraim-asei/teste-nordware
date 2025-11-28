import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from '../views/Dashboard.vue'
import Register from '../views/Register.vue'
import { useAuthStore } from '../stores/auth'

import StockOffices from '../views/StockOffices.vue'
import Colors from '../views/Colors.vue'
import Sizes from '../views/Sizes.vue'
import Products from '../views/Products.vue'
import CreateOrder from '../views/CreateOrder.vue'
import Skus from '../views/Skus.vue'
import CreateSku from '../views/CreateSku.vue'
import Stocks from '../views/Stocks.vue'
import CreateStock from '../views/CreateStock.vue'
import PriceTables from '../views/PriceTables.vue'
import ProductPrices from '../views/ProductPrices.vue'
import Customers from '../views/Customers.vue'

const routes = [
  {
    path: '/',
    name: 'Dashboard',
    component: Dashboard,
    meta: { requiresAuth: true }
  },
  {
    path: '/register',
    name: 'Register',
    component: Register,
    meta: { requiresAuth: false }
  },
  {
    path: '/login',
    name: 'Login',
    component: Dashboard, // Redireciona para dashboard que mostra o modal de login
    meta: { requiresAuth: false }
  },
  {
    path: '/stock-offices',
    name: 'StockOffices',
    component: StockOffices,
    meta: { requiresAuth: true }
  },
  {
    path: '/colors',
    name: 'Colors',
    component: Colors,
    meta: { requiresAuth: true }
  },
  {
    path: '/sizes',
    name: 'Sizes',
    component: Sizes,
    meta: { requiresAuth: true }
  },
  {
    path: '/products',
    name: 'Products',
    component: Products,
    meta: { requiresAuth: true }
  },
  {
    path: '/create-order',
    name: 'CreateOrder',
    component: CreateOrder,
    meta: { requiresAuth: true }
  },
  {
    path: '/skus',
    name: 'Skus',
    component: Skus,
    meta: { requiresAuth: true }
  },
  {
    path: '/create-sku',
    name: 'CreateSku',
    component: CreateSku,
    meta: { requiresAuth: true }
  },
  {
    path: '/stocks',
    name: 'Stocks',
    component: Stocks,
    meta: { requiresAuth: true }
  },
  {
    path: '/create-stock',
    name: 'CreateStock',
    component: CreateStock,
    meta: { requiresAuth: true }
  },
  {
    path: '/price-tables',
    name: 'PriceTables',
    component: PriceTables,
    meta: { requiresAuth: true }
  },
  {
    path: '/product-prices',
    name: 'ProductPrices',
    component: ProductPrices,
    meta: { requiresAuth: true }
  },
  {
    path: '/customers',
    name: 'Customers',
    component: Customers,
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  // Se a rota requer autenticação e o usuário não está autenticado
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    // Se estiver tentando acessar uma rota protegida, mostrar modal de login
    // Mas permitir acesso à rota para que o App.vue mostre o modal
    next()
  } else {
    // Se já está autenticado e tenta acessar login/register, redirecionar para dashboard
    if (authStore.isAuthenticated && (to.name === 'Login' || to.name === 'Register')) {
      next('/')
    } else {
      next()
    }
  }
})

export default router


