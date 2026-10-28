<script setup lang="ts">
import type { Product } from '../types/Product'

defineProps<{ product: Product; busy: boolean; error: string }>()
const emit = defineEmits<{ confirm: []; cancel: [] }>()
</script>

<template>
  <div class="modal-backdrop" @click.self="emit('cancel')">
    <div class="modal" role="alertdialog" aria-modal="true" aria-labelledby="delete-title">
      <h2 id="delete-title">حذف المنتج</h2>
      <p class="modal-hint">
        هل تريد حذف «<b>{{ product.name }}</b
        >»؟ سيختفي من المتجر.
      </p>

      <p v-if="error" class="form-error" role="alert">{{ error }}</p>

      <div class="modal-actions">
        <button type="button" class="btn btn-secondary" @click="emit('cancel')">إلغاء</button>
        <button type="button" class="btn btn-danger" :disabled="busy" @click="emit('confirm')">
          {{ busy ? 'جارٍ الحذف...' : 'حذف' }}
        </button>
      </div>
    </div>
  </div>
</template>
