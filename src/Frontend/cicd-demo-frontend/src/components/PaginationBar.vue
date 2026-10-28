<script setup lang="ts">
const props = defineProps<{ page: number; totalPages: number }>()
const emit = defineEmits<{ 'update:page': [page: number] }>()

function go(target: number) {
  if (target < 1 || target > props.totalPages || target === props.page) return
  emit('update:page', target)
}
</script>

<template>
  <nav v-if="totalPages > 1" class="pagination" aria-label="التنقل بين الصفحات">
    <button class="page-button" :disabled="page <= 1" @click="go(page - 1)">السابق</button>

    <span class="page-status">صفحة {{ page }} من {{ totalPages }}</span>

    <button class="page-button" :disabled="page >= totalPages" @click="go(page + 1)">التالي</button>
  </nav>
</template>
