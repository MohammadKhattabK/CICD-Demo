import api from './api'
import { getAdminKey } from './adminKey'
import type { Product, ProductInput, ProductStats, ProductsResponse } from '../types/Product'

export interface ProductFilters {
  search?: string
  category?: string
  minPrice?: number
  maxPrice?: number
  inStock?: boolean
  sortBy?: 'price' | 'name' | 'id'
  sortOrder?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export async function getProducts(filters: ProductFilters = {}): Promise<ProductsResponse> {
  const { data } = await api.get<ProductsResponse>('/api/products', {
    params: filters,
  })

  return data
}

export async function getProductById(id: number): Promise<Product> {
  const { data } = await api.get<Product>(`/api/products/${id}`)
  return data
}

export async function getProductCategories(): Promise<string[]> {
  const { data } = await api.get<string[]>('/api/products/categories')
  return data
}

export async function getProductStats(): Promise<ProductStats> {
  const { data } = await api.get<ProductStats>('/api/products/stats')
  return data
}

function adminHeaders(key = getAdminKey()) {
  return { 'X-Api-Key': key }
}

/** Resolves when the key is valid, rejects with a 401 otherwise. */
export async function checkAdminKey(key: string): Promise<void> {
  await api.get('/api/admin/check', { headers: adminHeaders(key) })
}

export async function createProduct(input: ProductInput): Promise<Product> {
  const { data } = await api.post<Product>('/api/products', input, { headers: adminHeaders() })
  return data
}

export async function updateProduct(id: number, input: ProductInput): Promise<Product> {
  const { data } = await api.put<Product>(`/api/products/${id}`, input, {
    headers: adminHeaders(),
  })
  return data
}

export async function deleteProduct(id: number): Promise<void> {
  await api.delete(`/api/products/${id}`, { headers: adminHeaders() })
}
