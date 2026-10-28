<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import ProductCard from './components/ProductCard.vue'
import PaginationBar from './components/PaginationBar.vue'
import AdminLoginModal from './components/AdminLoginModal.vue'
import ProductFormModal from './components/ProductFormModal.vue'
import ConfirmDeleteModal from './components/ConfirmDeleteModal.vue'
import {
  checkAdminKey,
  createProduct,
  deleteProduct,
  getProducts,
  getProductCategories,
  updateProduct,
  type ProductFilters,
} from './services/productService'
import { clearAdminKey, getAdminKey, setAdminKey } from './services/adminKey'
import { getErrorMessage, isUnauthorized } from './services/errors'
import type { Product, ProductInput } from './types/Product'
import './style.css'
import './admin.css'

const products = ref<Product[]>([])
const categories = ref<string[]>([])
const search = ref('')
const category = ref('')
const inStock = ref(false)
const sortBy = ref<'price' | 'name'>('name')
const sortOrder = ref<'asc' | 'desc'>('asc')
const loading = ref(false)
const error = ref('')

const totalCount = ref(0)
const page = ref(1)
const totalPages = ref(1)
const PAGE_SIZE = 6
let requestId = 0

const filters = computed<ProductFilters>(() => ({
  search: search.value.trim() || undefined,
  category: category.value || undefined,
  inStock: inStock.value ? true : undefined,
  sortBy: sortBy.value,
  sortOrder: sortOrder.value,
  page: page.value,
  pageSize: PAGE_SIZE,
}))

async function loadProducts() {
  const currentRequestId = ++requestId

  loading.value = true
  error.value = ''

  try {
    const result = await getProducts(filters.value)

    if (currentRequestId !== requestId) return

    products.value = result.items
    totalCount.value = result.totalCount
    totalPages.value = result.totalPages
  } catch {
    if (currentRequestId !== requestId) return

    error.value = 'تعذر تحميل المنتجات. تأكد من تشغيل الـ API وإعداد عنوان الاتصال.'
  } finally {
    if (currentRequestId === requestId) {
      loading.value = false
    }
  }
}

// Any filter change goes back to the first page; page changes just reload.
watch([search, category, inStock, sortBy, sortOrder], () => {
  if (page.value !== 1) {
    page.value = 1 // triggers the filters watcher below
  }
})

watch(filters, loadProducts)

async function loadCategories() {
  try {
    categories.value = await getProductCategories()
  } catch {
    error.value = 'تعذر تحميل التصنيفات.'
  }
}

onMounted(() => Promise.all([loadProducts(), loadCategories()]))

// --- Admin: create / edit / delete -------------------------------------------------

const isAdmin = ref(Boolean(getAdminKey()))
const showLogin = ref(false)
const showForm = ref(false)
const editing = ref<Product | null>(null)
const deleting = ref<Product | null>(null)
const busy = ref(false)
const dialogError = ref('')
const notice = ref('')
let noticeTimer: ReturnType<typeof setTimeout> | undefined

function showNotice(message: string) {
  notice.value = message
  clearTimeout(noticeTimer)
  noticeTimer = setTimeout(() => (notice.value = ''), 4000)
}

function closeDialogs() {
  showLogin.value = false
  showForm.value = false
  editing.value = null
  deleting.value = null
  dialogError.value = ''
}

function logout() {
  clearAdminKey()
  isAdmin.value = false
  closeDialogs()
}

function openLogin() {
  dialogError.value = ''
  showLogin.value = true
}

/** A wrong or expired key during a write sends the user back to the login dialog. */
function handleWriteError(err: unknown) {
  if (isUnauthorized(err)) {
    logout()
    showLogin.value = true
  }

  dialogError.value = getErrorMessage(err)
}

async function login(key: string) {
  busy.value = true
  dialogError.value = ''

  try {
    await checkAdminKey(key)
    setAdminKey(key)
    isAdmin.value = true
    closeDialogs()
    showNotice('تم تسجيل الدخول كمسؤول.')
  } catch (err) {
    dialogError.value = getErrorMessage(err)
  } finally {
    busy.value = false
  }
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

async function refreshAfterChange() {
  await Promise.all([loadProducts(), loadCategories()])
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
    await refreshAfterChange()
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
      page.value -= 1 // the filters watcher reloads the products
      await loadCategories()
    } else {
      await refreshAfterChange()
    }
  } catch (err) {
    handleWriteError(err)
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <main class="store-page">
    <header class="topbar">
      <a class="brand" href="#" aria-label="CICD Demo">
        <span class="brand-mark">C</span>
        <span>CICD <b>Demo</b></span>
      </a>

      <div class="topbar-actions">
        <span class="topbar-label">متجر المنتجات</span>

        <template v-if="isAdmin">
          <button type="button" class="btn btn-primary btn-small" @click="openCreate">
            + إضافة منتج
          </button>
          <button type="button" class="btn btn-secondary btn-small" @click="logout">
            تسجيل الخروج
          </button>
        </template>
        <button v-else type="button" class="btn btn-secondary btn-small" @click="openLogin">
          دخول المسؤول
        </button>
      </div>
    </header>

    <section class="hero">
      <div class="hero-content">
        <span class="eyebrow">CICD DEMO STORE</span>
        <h1>اكتشف منتجاتنا</h1>
        <p>مجموعة متنوعة من المنتجات المختارة بعناية لتناسب احتياجاتك اليومية.</p>

        <a href="#products" class="hero-button">
          استكشف المنتجات
          <span aria-hidden="true">←</span>
        </a>
      </div>

      <div class="hero-visual" aria-hidden="true">
        <div class="hero-circle circle-one"></div>
        <div class="hero-circle circle-two"></div>
        <div class="hero-product">✦</div>
        <div class="hero-decoration">✧</div>
      </div>
    </section>

    <section class="catalog-section" id="products">
      <div class="section-heading">
        <div>
          <h2>منتجاتنا</h2>
          <p>تصفح مجموعتنا واعثر على ما تبحث عنه.</p>
        </div>

        <span class="results-count"> {{ totalCount }} منتج </span>
      </div>

      <div class="filters-panel">
        <label class="search-box">
          <svg
            width="19"
            height="19"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.8"
            aria-hidden="true"
          >
            <circle cx="11" cy="11" r="7" />
            <path d="m16 16 4 4" />
          </svg>

          <input
            v-model="search"
            type="search"
            placeholder="ابحث عن منتج..."
            aria-label="البحث عن منتج"
          />
        </label>

        <select v-model="category" aria-label="تصنيف المنتجات">
          <option value="">كل التصنيفات</option>
          <option v-for="item in categories" :key="item" :value="item">
            {{ item }}
          </option>
        </select>

        <select v-model="sortBy" aria-label="ترتيب المنتجات">
          <option value="name">ترتيب حسب الاسم</option>
          <option value="price">ترتيب حسب السعر</option>
        </select>

        <select v-model="sortOrder" aria-label="اتجاه الترتيب">
          <option value="asc">تصاعدي</option>
          <option value="desc">تنازلي</option>
        </select>

        <label class="stock-filter">
          <input v-model="inStock" type="checkbox" />
          <span>المتوفر فقط</span>
        </label>
      </div>

      <div v-if="loading" class="state-panel">
        <span class="loading-spinner"></span>
        <p>جارٍ تحميل المنتجات...</p>
      </div>

      <div v-else-if="error" class="state-panel error-panel">
        <p>{{ error }}</p>
        <button class="retry-button" @click="loadProducts">إعادة المحاولة</button>
      </div>

      <div v-else-if="products.length" class="product-grid">
        <ProductCard
          v-for="product in products"
          :key="product.id"
          :product="product"
          :admin="isAdmin"
          @edit="openEdit"
          @delete="openDelete"
        />
      </div>

      <PaginationBar v-if="!loading && !error" v-model:page="page" :total-pages="totalPages" />

      <div v-else class="state-panel">
        <p>لا توجد منتجات مطابقة للبحث الحالي.</p>
      </div>

      <footer class="page-footer">
        <span>© {{ new Date().getFullYear() }} CICD Demo | CI/CD Live Deployment Test</span>
        <span>واجهة تجريبية لعرض المنتجات</span>
      </footer>
    </section>

    <AdminLoginModal
      v-if="showLogin"
      :busy="busy"
      :error="dialogError"
      @submit="login"
      @cancel="closeDialogs"
    />

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
  </main>
</template>
