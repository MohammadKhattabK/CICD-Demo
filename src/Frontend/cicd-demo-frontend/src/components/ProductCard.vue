<script setup lang="ts">
import type { Product } from '../types/Product'

defineProps<{ product: Product }>()

const frontendBaseUrl = import.meta.env.BASE_URL

const priceFormat = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
})

function resolveImageUrl(path: string) {
  if (!path) return `${frontendBaseUrl}images/product-placeholder.svg`

  if (/^https?:\/\//i.test(path)) return path

  return `${frontendBaseUrl}${path.replace(/^\/+/, '')}`
}

function handleImageError(event: Event) {
  const image = event.target as HTMLImageElement
  image.onerror = null
  image.src = 'https://placehold.co/600x400/eef0ff/4f46e5?text=Product'
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
        <strong class="product-price">{{ priceFormat.format(product.price) }}</strong>

        <span class="badge" :class="product.stock > 0 ? 'badge-success' : 'badge-danger'">
          {{ product.stock > 0 ? `${product.stock} متوفر` : 'نفد المخزون' }}
        </span>
      </div>
    </div>
  </article>
</template>
