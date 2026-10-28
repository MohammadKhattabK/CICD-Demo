export interface Product {
  id: number
  name: string
  description: string
  price: number
  category: string
  stock: number
  isActive: boolean
  imageUrl: string
}

export interface ProductsResponse {
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  items: Product[]
}

/** Body sent when creating or replacing a product. */
export interface ProductInput {
  name: string
  description: string
  price: number
  category: string
  stock: number
  imageUrl: string
}

export interface ProductStats {
  totalProducts: number
  inStock: number
  outOfStock: number
  categories: number
  inventoryValue: number
}
