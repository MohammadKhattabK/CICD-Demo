<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import StorePage from './views/StorePage.vue'
import AdminPage from './views/AdminPage.vue'

// Hash routing keeps the static site working without server-side rewrites.
// The store lives at "#/" and the admin panel at "#/admin".
const hash = ref(window.location.hash)
const isAdminRoute = computed(() => hash.value.startsWith('#/admin'))

function syncHash() {
  hash.value = window.location.hash
}

onMounted(() => window.addEventListener('hashchange', syncHash))
onUnmounted(() => window.removeEventListener('hashchange', syncHash))
</script>

<template>
  <AdminPage v-if="isAdminRoute" />
  <StorePage v-else />
</template>
