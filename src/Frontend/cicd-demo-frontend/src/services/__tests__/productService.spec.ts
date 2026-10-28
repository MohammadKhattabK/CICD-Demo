import { beforeEach, describe, expect, it, vi } from 'vitest'
import { AxiosError, type AxiosResponse } from 'axios'

vi.mock('../api', () => ({
  default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}))

import api from '../api'
import { clearAdminKey, getAdminKey, setAdminKey } from '../adminKey'
import { getErrorMessage, isUnauthorized } from '../errors'
import { checkAdminKey, createProduct, deleteProduct, updateProduct } from '../productService'
import type { ProductInput } from '../../types/Product'

const input: ProductInput = {
  name: 'Desk',
  description: '',
  price: 10,
  category: 'Home',
  stock: 1,
  imageUrl: '',
}

function httpError(status: number, data: unknown = {}) {
  return new AxiosError('failed', String(status), undefined, undefined, {
    status,
    data,
  } as AxiosResponse)
}

describe('product write calls', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    sessionStorage.clear()
  })

  it('sends the stored admin key with create, update and delete', async () => {
    vi.mocked(api.post).mockResolvedValue({ data: { id: 1 } })
    vi.mocked(api.put).mockResolvedValue({ data: { id: 1 } })
    vi.mocked(api.delete).mockResolvedValue({})
    setAdminKey('my-key')

    await createProduct(input)
    await updateProduct(1, input)
    await deleteProduct(1)

    const headers = { headers: { 'X-Api-Key': 'my-key' } }
    expect(api.post).toHaveBeenCalledWith('/api/products', input, headers)
    expect(api.put).toHaveBeenCalledWith('/api/products/1', input, headers)
    expect(api.delete).toHaveBeenCalledWith('/api/products/1', headers)
  })

  it('checks a candidate key without storing it', async () => {
    vi.mocked(api.get).mockResolvedValue({})

    await checkAdminKey('candidate')

    expect(api.get).toHaveBeenCalledWith('/api/admin/check', {
      headers: { 'X-Api-Key': 'candidate' },
    })
    expect(getAdminKey()).toBe('')
  })

  it('stores and clears the admin key', () => {
    setAdminKey('abc')
    expect(getAdminKey()).toBe('abc')

    clearAdminKey()
    expect(getAdminKey()).toBe('')
  })
})

describe('getErrorMessage', () => {
  it('explains an invalid key', () => {
    expect(getErrorMessage(httpError(401))).toContain('مفتاح الإدارة')
    expect(isUnauthorized(httpError(401))).toBe(true)
    expect(isUnauthorized(httpError(500))).toBe(false)
  })

  it('lists server validation errors', () => {
    const error = httpError(400, { errors: { name: ['Name is required.'], stock: ['Bad stock.'] } })

    expect(getErrorMessage(error)).toBe('Name is required. Bad stock.')
  })

  it('handles network failures and unknown errors', () => {
    expect(getErrorMessage(new AxiosError('Network Error'))).toContain('تعذر الاتصال')
    expect(getErrorMessage(new Error('boom'))).toBe('حدث خطأ غير متوقع.')
  })
})
