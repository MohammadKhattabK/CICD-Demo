
import api from './api'
import type { Product, ProductsResponse } from '../types/Product'

export interface ProductFilters {
  search?: string
  category?: string
  minPrice?: number
  maxPrice?: number
  inStock?: boolean
  sortBy?: 'price' | 'name' | 'id'
  sortOrder?: 'asc' | 'desc'
}

export async function getProducts(
  filters: ProductFilters = {},
): Promise<ProductsResponse> {
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
