<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch } from 'vue'
import AdminLoginForm from '../components/AdminLoginForm.vue'
import ProductFormModal from '../components/ProductFormModal.vue'
import ConfirmDeleteModal from '../components/ConfirmDeleteModal.vue'
import PaginationBar from '../components/PaginationBar.vue'
import {
  checkAdminKey,
  createProduct,
  deleteProduct,
  getProductCategories,
  getProducts,
  getProductStats,
  updateProduct,
} from '../services/productService'
import { clearAdminKey, getAdminKey, setAdminKey } from '../services/adminKey'
import { getErrorMessage, isUnauthorized } from '../services/errors'
import type { Product, ProductInput, ProductStats } from '../types/Product'

const PAGE_SIZE = 10

const isAdmin = ref(false)
const checkingSession = ref(Boolean(getAdminKey()))
const loginError = ref('')
const busy = ref(false)

const products = ref<Product[]>([])
const categories = ref<string[]>([])
const stats = ref<ProductStats | null>(null)
const search = ref('')
const page = ref(1)
const totalPages = ref(1)
const totalCount = ref(0)
const loading = ref(false)
const loadError = ref('')

const showForm = ref(false)
const editing = ref<Product | null>(null)
const deleting = ref<Product | null>(null)
const dialogError = ref('')
const notice = ref('')

let requestId = 0
let noticeTimer: ReturnType<typeof setTimeout> | undefined
let searchTimer: ReturnType<typeof setTimeout> | undefined

const money = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

function showNotice(message: string) {
  notice.value = message
  clearTimeout(noticeTimer)
  noticeTimer = setTimeout(() => (notice.value = ''), 4000)
}

function closeDialogs() {
  showForm.value = false
  editing.value = null
  deleting.value = null
  dialogError.value = ''
}

function logout() {
  clearAdminKey()
  isAdmin.value = false
  loginError.value = ''
  products.value = []
  stats.value = null
  closeDialogs()
}

async function loadProducts() {
  const currentRequestId = ++requestId
  loading.value = true
  loadError.value = ''

  try {
    const result = await getProducts({
      search: search.value.trim() || undefined,
      sortBy: 'id',
      sortOrder: 'desc',
      page: page.value,
      pageSize: PAGE_SIZE,
    })

    if (currentRequestId !== requestId) return

    products.value = result.items
    totalCount.value = result.totalCount
    totalPages.value = result.totalPages
  } catch {
    if (currentRequestId !== requestId) return
    loadError.value = 'تعذر تحميل المنتجات.'
  } finally {
    if (currentRequestId === requestId) loading.value = false
  }
}

async function loadSideData() {
  try {
    const [loadedStats, loadedCategories] = await Promise.all([
      getProductStats(),
      getProductCategories(),
    ])
    stats.value = loadedStats
    categories.value = loadedCategories
  } catch {
    /* the product list shows its own error; stats are optional */
  }
}

function refreshAll() {
  return Promise.all([loadProducts(), loadSideData()])
}

async function login(key: string) {
  busy.value = true
  loginError.value = ''

  try {
    await checkAdminKey(key)
    setAdminKey(key)
    isAdmin.value = true
  } catch (err) {
    loginError.value = getErrorMessage(err)
  } finally {
    busy.value = false
  }
}

/** A wrong or expired key during a write sends the user back to the login screen. */
function handleWriteError(err: unknown) {
  if (isUnauthorized(err)) {
    logout()
    loginError.value = getErrorMessage(err)
    return
  }

  dialogError.value = getErrorMessage(err)
}

function openCreate() {
  editing.value = null
  dialogError.value = ''
  showForm.value = true
}

function openEdit(product: Product) {
  editing.value = product
  dialogError.value = ''
  showForm.value = true
}

function openDelete(product: Product) {
  dialogError.value = ''
  deleting.value = product
}

async function saveProduct(input: ProductInput) {
  const target = editing.value
  busy.value = true
  dialogError.value = ''

  try {
    if (target) {
      await updateProduct(target.id, input)
    } else {
      await createProduct(input)
    }

    closeDialogs()
    showNotice(target ? 'تم تحديث المنتج.' : 'تمت إضافة المنتج.')
    await refreshAll()
  } catch (err) {
    handleWriteError(err)
  } finally {
    busy.value = false
  }
}

async function confirmDelete() {
  const target = deleting.value
  if (!target) return

  busy.value = true
  dialogError.value = ''

  try {
    await deleteProduct(target.id)

    // Deleting the last item of a later page would leave an empty page.
    const lastOnPage = products.value.length === 1 && page.value > 1

    closeDialogs()
    showNotice('تم حذف المنتج.')

    if (lastOnPage) {
      page.value -= 1 // the page watcher reloads the products
      await loadSideData()
    } else {
      await refreshAll()
    }
  } catch (err) {
    handleWriteError(err)
  } finally {
    busy.value = false
  }
}

watch(isAdmin, (value) => {
  if (value) {
    page.value = 1
    refreshAll()
  }
})

watch(page, () => {
  if (isAdmin.value) loadProducts()
})

// Debounced so that typing does not fire a request per keystroke.
watch(search, () => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    if (!isAdmin.value) return
    if (page.value !== 1) page.value = 1
    else loadProducts()
  }, 300)
})

onMounted(async () => {
  const key = getAdminKey()

  if (key) {
    try {
      await checkAdminKey(key)
      isAdmin.value = true
    } catch {
      clearAdminKey() // stale session
    }
  }

  checkingSession.value = false
})

onUnmounted(() => {
  clearTimeout(noticeTimer)
  clearTimeout(searchTimer)
})
</script>

<template>
  <div class="page admin-page">
    <header class="site-header">
      <div class="container header-inner">
        <a class="brand" href="#/" aria-label="CICD Demo">
          <span class="brand-mark">C</span>
          <span>CICD <b>Demo</b></span>
        </a>

        <nav class="header-actions">
          <span class="header-label">لوحة الإدارة</span>
          <a class="btn btn-secondary btn-small" href="#/">المتجر</a>
          <button v-if="isAdmin" type="button" class="btn btn-secondary btn-small" @click="logout">
            تسجيل الخروج
          </button>
        </nav>
      </div>
    </header>

    <main class="container">
      <div v-if="checkingSession" class="state-panel admin-content">
        <span class="loading-spinner"></span>
        <p>جارٍ التحقق من الجلسة...</p>
      </div>

      <section v-else-if="!isAdmin" class="login-wrapper">
        <AdminLoginForm :busy="busy" :error="loginError" @submit="login" />
      </section>

      <section v-else class="admin-content">
        <div class="admin-heading">
          <div>
            <h1>إدارة المنتجات</h1>
            <p>أضف المنتجات وعدّلها وتحكم في المخزون.</p>
          </div>
          <button type="button" class="btn btn-primary" @click="openCreate">+ إضافة منتج</button>
        </div>

        <div v-if="stats" class="stats-grid">
          <div class="stat-card">
            <span>إجمالي المنتجات</span>
            <strong>{{ stats.totalProducts }}</strong>
          </div>
          <div class="stat-card">
            <span>متوفر</span>
            <strong>{{ stats.inStock }}</strong>
          </div>
          <div class="stat-card">
            <span>نفد المخزون</span>
            <strong>{{ stats.outOfStock }}</strong>
          </div>
          <div class="stat-card">
            <span>قيمة المخزون</span>
            <strong>{{ money.format(stats.inventoryValue) }}</strong>
          </div>
        </div>

        <div class="admin-toolbar">
          <input
            v-model="search"
            class="input admin-search"
            type="search"
            placeholder="ابحث في المنتجات..."
            aria-label="البحث في المنتجات"
          />
        </div>

        <div v-if="loading" class="state-panel">
          <span class="loading-spinner"></span>
          <p>جارٍ تحميل المنتجات...</p>
        </div>

        <div v-else-if="loadError" class="state-panel error-panel">
          <p>{{ loadError }}</p>
          <button class="retry-button" @click="loadProducts">إعادة المحاولة</button>
        </div>

        <div v-else-if="products.length" class="table-wrapper">
          <table class="admin-table">
            <thead>
              <tr>
                <th>#</th>
                <th>الاسم</th>
                <th>التصنيف</th>
                <th>السعر</th>
                <th>المخزون</th>
                <th>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="product in products" :key="product.id">
                <td class="cell-muted">{{ product.id }}</td>
                <td class="cell-name">{{ product.name }}</td>
                <td class="cell-muted">{{ product.category }}</td>
                <td class="cell-price">{{ money.format(product.price) }}</td>
                <td>
                  <span class="badge" :class="product.stock > 0 ? 'badge-success' : 'badge-danger'">
                    {{ product.stock > 0 ? product.stock : 'نفد' }}
                  </span>
                </td>
                <td class="row-actions">
                  <button
                    type="button"
                    class="btn btn-secondary btn-small"
                    @click="openEdit(product)"
                  >
                    تعديل
                  </button>
                  <button
                    type="button"
                    class="btn btn-ghost-danger btn-small"
                    @click="openDelete(product)"
                  >
                    حذف
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="state-panel">
          <p>لا توجد منتجات.</p>
        </div>

        <PaginationBar
          v-if="!loading && !loadError"
          v-model:page="page"
          :total-pages="totalPages"
        />
      </section>
    </main>

    <ProductFormModal
      v-if="showForm"
      :product="editing"
      :categories="categories"
      :busy="busy"
      :error="dialogError"
      @save="saveProduct"
      @cancel="closeDialogs"
    />

    <ConfirmDeleteModal
      v-if="deleting"
      :product="deleting"
      :busy="busy"
      :error="dialogError"
      @confirm="confirmDelete"
      @cancel="closeDialogs"
    />

    <div v-if="notice" class="toast" role="status">{{ notice }}</div>
  </div>
</template>
