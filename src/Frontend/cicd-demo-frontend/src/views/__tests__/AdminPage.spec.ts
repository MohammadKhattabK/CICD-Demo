import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { AxiosError, type AxiosResponse } from 'axios'

vi.mock('../../services/productService', () => ({
  checkAdminKey: vi.fn(),
  getProducts: vi.fn(),
  getProductCategories: vi.fn(),
  getProductStats: vi.fn(),
  createProduct: vi.fn(),
  updateProduct: vi.fn(),
  deleteProduct: vi.fn(),
}))

import * as service from '../../services/productService'
import { getAdminKey, setAdminKey } from '../../services/adminKey'
import AdminPage from '../AdminPage.vue'
import type { Product } from '../../types/Product'

const product: Product = {
  id: 1,
  name: 'Desk Lamp',
  description: 'Adjustable lamp',
  price: 89,
  category: 'Home',
  stock: 8,
  isActive: true,
  imageUrl: '',
}

function unauthorized() {
  return new AxiosError('no', '401', undefined, undefined, { status: 401 } as AxiosResponse)
}

describe('AdminPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    sessionStorage.clear()

    vi.mocked(service.getProducts).mockResolvedValue({
      totalCount: 1,
      page: 1,
      pageSize: 10,
      totalPages: 1,
      items: [product],
    })
    vi.mocked(service.getProductCategories).mockResolvedValue(['Home'])
    vi.mocked(service.getProductStats).mockResolvedValue({
      totalProducts: 1,
      inStock: 1,
      outOfStock: 0,
      categories: 1,
      inventoryValue: 712,
    })
  })

  it('asks for the admin key and loads nothing before logging in', async () => {
    const wrapper = mount(AdminPage)
    await flushPromises()

    expect(wrapper.find('input[type="password"]').exists()).toBe(true)
    expect(wrapper.find('table').exists()).toBe(false)
    expect(service.getProducts).not.toHaveBeenCalled()
  })

  it('shows the product table after a valid login and remembers the key', async () => {
    vi.mocked(service.checkAdminKey).mockResolvedValue()
    const wrapper = mount(AdminPage)
    await flushPromises()

    await wrapper.find('input[type="password"]').setValue('good-key')
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(service.checkAdminKey).toHaveBeenCalledWith('good-key')
    expect(getAdminKey()).toBe('good-key')
    expect(wrapper.find('table').text()).toContain('Desk Lamp')
    expect(wrapper.find('.stats-grid').text()).toContain('712')
  })

  it('shows an error and stays logged out for a wrong key', async () => {
    vi.mocked(service.checkAdminKey).mockRejectedValue(unauthorized())
    const wrapper = mount(AdminPage)
    await flushPromises()

    await wrapper.find('input[type="password"]').setValue('bad-key')
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(wrapper.find('[role="alert"]').text()).toContain('مفتاح الإدارة غير صحيح')
    expect(getAdminKey()).toBe('')
    expect(wrapper.find('table').exists()).toBe(false)
  })

  it('restores a valid session without asking again', async () => {
    setAdminKey('saved-key')
    vi.mocked(service.checkAdminKey).mockResolvedValue()

    const wrapper = mount(AdminPage)
    await flushPromises()

    expect(wrapper.find('input[type="password"]').exists()).toBe(false)
    expect(wrapper.find('table').text()).toContain('Desk Lamp')
  })

  it('discards a stale session key', async () => {
    setAdminKey('old-key')
    vi.mocked(service.checkAdminKey).mockRejectedValue(unauthorized())

    const wrapper = mount(AdminPage)
    await flushPromises()

    expect(wrapper.find('input[type="password"]').exists()).toBe(true)
    expect(getAdminKey()).toBe('')
  })

  it('deletes a product after confirmation and reloads the list', async () => {
    setAdminKey('saved-key')
    vi.mocked(service.checkAdminKey).mockResolvedValue()
    vi.mocked(service.deleteProduct).mockResolvedValue()

    const wrapper = mount(AdminPage)
    await flushPromises()

    await wrapper.find('.row-actions .btn-danger').trigger('click')
    await wrapper.find('[role="alertdialog"] .btn-danger').trigger('click')
    await flushPromises()

    expect(service.deleteProduct).toHaveBeenCalledWith(1)
    expect(vi.mocked(service.getProducts).mock.calls.length).toBeGreaterThan(1)
    expect(wrapper.find('.toast').text()).toContain('تم حذف المنتج')
  })
})
