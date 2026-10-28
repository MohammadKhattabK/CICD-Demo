<script setup lang="ts">
import type { Product } from '../types/Product'

defineProps<{ product: Product; admin?: boolean }>()
defineEmits<{ edit: [product: Product]; delete: [product: Product] }>()

const frontendBaseUrl = import.meta.env.BASE_URL

function resolveImageUrl(path: string) {
  if (!path) return `${frontendBaseUrl}images/product-placeholder.svg`

  if (/^https?:\/\//i.test(path)) return path

  return `${frontendBaseUrl}${path.replace(/^\/+/, '')}`
}

function handleImageError(event: Event) {
  const image = event.target as HTMLImageElement
  image.onerror = null
  image.src = 'https://placehold.co/600x400/f0f5f2/008755?text=Product'
}
</script>

<template>
  <article class="product-card">
    <div class="product-image-wrapper">
      <img
        :src="resolveImageUrl(product.imageUrl)"
        :alt="product.name"
        class="product-image"
        loading="lazy"
        @error="handleImageError"
      />

      <span class="category-badge">{{ product.category }}</span>
    </div>

    <div class="product-content">
      <h3>{{ product.name }}</h3>
      <p class="product-description">{{ product.description }}</p>

      <div class="product-footer">
        <div class="product-meta">
          <strong class="product-price">
            {{
              new Intl.NumberFormat('en-US', {
                style: 'currency',
                currency: 'USD',
              }).format(product.price)
            }}
          </strong>

          <span class="stock-badge" :class="product.stock > 0 ? 'in-stock' : 'out-of-stock'">
            <span class="stock-dot"></span>
            {{ product.stock > 0 ? `${product.stock} متوفر` : 'نفد المخزون' }}
          </span>
        </div>

        <div class="cart-icon" aria-label="المنتج">
          <svg
            width="21"
            height="21"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.8"
            stroke-linecap="round"
            stroke-linejoin="round"
            aria-hidden="true"
          >
            <circle cx="9" cy="21" r="1" />
            <circle cx="20" cy="21" r="1" />
            <path d="M1 1h4l2.68 13.39a2 2 0 002 1.61h9.72a2 2 0 002-1.61L23 6H6" />
          </svg>
        </div>
      </div>

      <div v-if="admin" class="admin-actions">
        <button type="button" class="btn btn-secondary btn-small" @click="$emit('edit', product)">
          تعديل
        </button>
        <button type="button" class="btn btn-danger btn-small" @click="$emit('delete', product)">
          حذف
        </button>
      </div>
    </div>
  </article>
</template>
