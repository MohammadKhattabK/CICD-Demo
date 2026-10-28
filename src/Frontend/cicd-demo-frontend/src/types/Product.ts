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
  items: Product[]
}
