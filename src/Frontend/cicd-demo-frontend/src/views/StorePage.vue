<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import ProductCard from '../components/ProductCard.vue'
import PaginationBar from '../components/PaginationBar.vue'
import { getProducts, getProductCategories, type ProductFilters } from '../services/productService'
import type { Product } from '../types/Product'

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
const base = import.meta.env.BASE_URL
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
</script>

<template>
  <div class="page">
    <header class="site-header">
      <div class="container header-inner">
        <a class="brand" href="#/" aria-label="CICD Demo">
          <span class="brand-mark">C</span>
          <span>CICD <b>Demo</b></span>
        </a>

        <nav class="header-actions">
          <span class="header-label">متجر المنتجات</span>
          <a class="btn btn-secondary btn-small" href="#/admin">لوحة الإدارة</a>
        </nav>
      </div>
    </header>

    <main>
      <section class="hero">
        <div class="container hero-grid">
          <div class="hero-content">
            <span class="eyebrow">CICD Demo Store</span>
            <h1>اكتشف منتجاتنا</h1>
            <p class="hero-lead">
              مجموعة متنوعة من المنتجات المختارة بعناية لتناسب احتياجاتك اليومية.
            </p>

            <a href="#products" class="btn btn-primary btn-large">
              استكشف المنتجات
              <span aria-hidden="true">←</span>
            </a>
          </div>

          <div class="hero-visual" aria-hidden="true">
            <img class="hero-img hero-img-1" :src="`${base}images/headphones.jpg`" alt="" />
            <img class="hero-img hero-img-2" :src="`${base}images/smartwatch.jpg`" alt="" />
            <img class="hero-img hero-img-3" :src="`${base}images/keyboard.jpg`" alt="" />
          </div>
        </div>
      </section>

      <div class="container benefits">
        <div class="benefit">
          <span class="benefit-icon" aria-hidden="true">
            <svg
              width="22"
              height="22"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="1.8"
              stroke-linecap="round"
              stroke-linejoin="round"
            >
              <path d="M3 7h11v9H3zM14 10h4l3 3v3h-7z" />
              <circle cx="7" cy="18" r="1.6" />
              <circle cx="17" cy="18" r="1.6" />
            </svg>
          </span>
          <div>
            <strong>شحن سريع</strong>
            <span>توصيل موثوق لباب بيتك</span>
          </div>
        </div>

        <div class="benefit">
          <span class="benefit-icon" aria-hidden="true">
            <svg
              width="22"
              height="22"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="1.8"
              stroke-linecap="round"
              stroke-linejoin="round"
            >
              <path d="M12 3l7 3v5c0 4.5-3 8-7 10-4-2-7-5.5-7-10V6z" />
              <path d="m9 12 2 2 4-4" />
            </svg>
          </span>
          <div>
            <strong>جودة مضمونة</strong>
            <span>منتجات مختارة بعناية</span>
          </div>
        </div>

        <div class="benefit">
          <span class="benefit-icon" aria-hidden="true">
            <svg
              width="22"
              height="22"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="1.8"
              stroke-linecap="round"
              stroke-linejoin="round"
            >
              <path d="M4 13v-1a8 8 0 0 1 16 0v1" />
              <rect x="3" y="13" width="4" height="6" rx="1.5" />
              <rect x="17" y="13" width="4" height="6" rx="1.5" />
            </svg>
          </span>
          <div>
            <strong>دعم متواصل</strong>
            <span>نساعدك في أي وقت</span>
          </div>
        </div>
      </div>

      <section class="catalog-section container" id="products">
        <div class="section-heading">
          <div>
            <h2>منتجاتنا</h2>
            <p>تصفح مجموعتنا واعثر على ما تبحث عنه.</p>
          </div>

          <span class="results-count">{{ totalCount }} منتج</span>
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

        <div v-if="loading" class="product-grid" aria-busy="true" aria-label="جارٍ تحميل المنتجات">
          <div v-for="n in PAGE_SIZE" :key="n" class="skeleton skeleton-card"></div>
        </div>

        <div v-else-if="error" class="state-panel error-panel">
          <p>{{ error }}</p>
          <button class="retry-button" @click="loadProducts">إعادة المحاولة</button>
        </div>

        <div v-else-if="products.length" class="product-grid">
          <ProductCard v-for="product in products" :key="product.id" :product="product" />
        </div>

        <div v-else class="state-panel">
          <p>لا توجد منتجات مطابقة للبحث الحالي.</p>
        </div>

        <PaginationBar v-if="!loading && !error" v-model:page="page" :total-pages="totalPages" />
      </section>
    </main>

    <footer class="site-footer">
      <div class="container footer-inner">
        <span>© {{ new Date().getFullYear() }} CICD Demo | CI/CD Live Deployment Test</span>
        <span>واجهة تجريبية لعرض المنتجات</span>
      </div>
    </footer>
  </div>
</template>
