<script setup lang="ts">
import { computed, reactive } from 'vue'
import type { Product, ProductInput } from '../types/Product'

const props = defineProps<{
  product: Product | null
  categories: string[]
  busy: boolean
  error: string
}>()
const emit = defineEmits<{ save: [input: ProductInput]; cancel: [] }>()

const isEdit = computed(() => props.product !== null)

const form = reactive({
  name: props.product?.name ?? '',
  description: props.product?.description ?? '',
  price: props.product?.price ?? 0,
  category: props.product?.category ?? '',
  stock: props.product?.stock ?? 0,
  imageUrl: props.product?.imageUrl ?? '',
})

const errors = computed(() => {
  const result: Record<string, string> = {}

  if (!form.name.trim()) result.name = 'الاسم مطلوب.'
  else if (form.name.trim().length > 120) result.name = 'الاسم لا يزيد عن 120 حرفًا.'

  if (!form.category.trim()) result.category = 'التصنيف مطلوب.'

  if (!Number.isFinite(form.price) || form.price < 0 || form.price > 1_000_000)
    result.price = 'السعر يجب أن يكون بين 0 و 1,000,000.'

  if (!Number.isInteger(form.stock) || form.stock < 0)
    result.stock = 'المخزون يجب أن يكون عددًا صحيحًا لا يقل عن 0.'

  return result
})

const isValid = computed(() => Object.keys(errors.value).length === 0)

function save() {
  if (!isValid.value) return

  emit('save', {
    name: form.name.trim(),
    description: form.description.trim(),
    price: Number(form.price),
    category: form.category.trim(),
    stock: Number(form.stock),
    imageUrl: form.imageUrl.trim(),
  })
}
</script>

<template>
  <div class="modal-backdrop" @click.self="emit('cancel')">
    <form
      class="modal modal-wide"
      role="dialog"
      aria-modal="true"
      aria-labelledby="form-title"
      novalidate
      @submit.prevent="save"
    >
      <h2 id="form-title">{{ isEdit ? 'تعديل المنتج' : 'إضافة منتج' }}</h2>

      <label class="form-field">
        <span>الاسم</span>
        <input v-model="form.name" name="name" type="text" maxlength="120" />
        <small v-if="errors.name" class="field-error">{{ errors.name }}</small>
      </label>

      <label class="form-field">
        <span>الوصف</span>
        <textarea v-model="form.description" name="description" rows="3" maxlength="1000" />
      </label>

      <div class="form-row">
        <label class="form-field">
          <span>السعر ($)</span>
          <input v-model.number="form.price" name="price" type="number" min="0" step="0.01" />
          <small v-if="errors.price" class="field-error">{{ errors.price }}</small>
        </label>

        <label class="form-field">
          <span>المخزون</span>
          <input v-model.number="form.stock" name="stock" type="number" min="0" step="1" />
          <small v-if="errors.stock" class="field-error">{{ errors.stock }}</small>
        </label>
      </div>

      <label class="form-field">
        <span>التصنيف</span>
        <input v-model="form.category" name="category" type="text" list="category-options" />
        <datalist id="category-options">
          <option v-for="item in categories" :key="item" :value="item" />
        </datalist>
        <small v-if="errors.category" class="field-error">{{ errors.category }}</small>
      </label>

      <label class="form-field">
        <span>رابط الصورة (اختياري)</span>
        <input v-model="form.imageUrl" name="imageUrl" type="text" dir="ltr" />
      </label>

      <p v-if="error" class="form-error" role="alert">{{ error }}</p>

      <div class="modal-actions">
        <button type="button" class="btn btn-secondary" @click="emit('cancel')">إلغاء</button>
        <button type="submit" class="btn btn-primary" :disabled="busy || !isValid">
          {{ busy ? 'جارٍ الحفظ...' : 'حفظ' }}
        </button>
      </div>
    </form>
  </div>
</template>
