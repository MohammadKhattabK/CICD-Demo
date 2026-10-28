<script setup lang="ts">
import { ref } from 'vue'

defineProps<{ busy: boolean; error: string }>()
const emit = defineEmits<{ submit: [key: string]; cancel: [] }>()

const key = ref('')

function submit() {
  const value = key.value.trim()
  if (value) emit('submit', value)
}
</script>

<template>
  <div class="modal-backdrop" @click.self="emit('cancel')">
    <form
      class="modal"
      role="dialog"
      aria-modal="true"
      aria-labelledby="login-title"
      @submit.prevent="submit"
    >
      <h2 id="login-title">دخول المسؤول</h2>
      <p class="modal-hint">أدخل مفتاح الإدارة لإضافة المنتجات وتعديلها وحذفها.</p>

      <label class="form-field">
        <span>مفتاح الإدارة</span>
        <input v-model="key" type="password" autocomplete="off" autofocus required />
      </label>

      <p v-if="error" class="form-error" role="alert">{{ error }}</p>

      <div class="modal-actions">
        <button type="button" class="btn btn-secondary" @click="emit('cancel')">إلغاء</button>
        <button type="submit" class="btn btn-primary" :disabled="busy || !key.trim()">
          {{ busy ? 'جارٍ التحقق...' : 'دخول' }}
        </button>
      </div>
    </form>
  </div>
</template>
