<script setup lang="ts">
import { ref } from 'vue'

defineProps<{ busy: boolean; error: string }>()
const emit = defineEmits<{ submit: [key: string] }>()

const key = ref('')

function submit() {
  const value = key.value.trim()
  if (value) emit('submit', value)
}
</script>

<template>
  <form class="login-card" aria-labelledby="login-title" @submit.prevent="submit">
    <h1 id="login-title">لوحة الإدارة</h1>
    <p class="modal-hint">سجّل الدخول بمفتاح الإدارة لإدارة المنتجات.</p>

    <label class="form-field">
      <span>مفتاح الإدارة</span>
      <input v-model="key" type="password" autocomplete="off" autofocus required />
    </label>

    <p v-if="error" class="form-error" role="alert">{{ error }}</p>

    <button type="submit" class="btn btn-primary login-submit" :disabled="busy || !key.trim()">
      {{ busy ? 'جارٍ التحقق...' : 'تسجيل الدخول' }}
    </button>

    <a class="back-link" href="#/">← العودة إلى المتجر</a>
  </form>
</template>
