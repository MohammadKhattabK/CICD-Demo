<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import ProductCard from './components/ProductCard.vue'
import { getProducts, getProductCategories, type ProductFilters } from './services/productService'
import type { Product } from './types/Product'
import './style.css'

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
let requestId = 0

const filters = computed<ProductFilters>(() => ({
  search: search.value.trim() || undefined,
  category: category.value || undefined,
  inStock: inStock.value ? true : undefined,
  sortBy: sortBy.value,
  sortOrder: sortOrder.value,
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
  } catch {
    if (currentRequestId !== requestId) return

    error.value = 'تعذر تحميل المنتجات. تأكد من تشغيل الـ API وإعداد عنوان الاتصال.'
  } finally {
    if (currentRequestId === requestId) {
      loading.value = false
    }
  }
}

watch(filters, loadProducts)

onMounted(async () => {
  await Promise.all([
    loadProducts(),
    getProductCategories()
      .then((result) => {
        categories.value = result
      })
      .catch(() => {
        error.value = 'تعذر تحميل التصنيفات.'
      }),
  ])
})
</script>

<template>
  <main class="store-page">
    <header class="topbar">
      <a class="brand" href="#" aria-label="CICD Demo">
        <span class="brand-mark">C</span>
        <span>CICD <b>Demo</b></span>
      </a>

      <span class="topbar-label">متجر المنتجات</span>
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
        <ProductCard v-for="product in products" :key="product.id" :product="product" />
      </div>

      <div v-else class="state-panel">
        <p>لا توجد منتجات مطابقة للبحث الحالي.</p>
      </div>

      <footer class="page-footer">
        <span>© {{ new Date().getFullYear() }} CICD Demo | CI/CD Live Deployment Test</span>
        <span>واجهة تجريبية لعرض المنتجات</span>
      </footer>
    </section>
  </main>
</template>
