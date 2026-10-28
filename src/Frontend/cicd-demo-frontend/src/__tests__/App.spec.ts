import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'

vi.mock('../services/productService', () => ({
  checkAdminKey: vi.fn(),
  getProducts: vi.fn().mockResolvedValue({
    totalCount: 0,
    page: 1,
    pageSize: 6,
    totalPages: 0,
    items: [],
  }),
  getProductCategories: vi.fn().mockResolvedValue([]),
  getProductStats: vi.fn(),
  createProduct: vi.fn(),
  updateProduct: vi.fn(),
  deleteProduct: vi.fn(),
}))

import App from '../App.vue'

describe('App routing', () => {
  beforeEach(() => {
    sessionStorage.clear()
  })

  it('shows the public store at the root without any admin controls', async () => {
    window.location.hash = ''
    const wrapper = mount(App)
    await flushPromises()

    expect(wrapper.text()).toContain('اكتشف منتجاتنا')
    expect(wrapper.find('input[type="password"]').exists()).toBe(false)
    expect(wrapper.find('a[href="#/admin"]').exists()).toBe(true)
  })

  it('shows the admin login at #/admin', async () => {
    window.location.hash = '#/admin'
    const wrapper = mount(App)
    await flushPromises()

    expect(wrapper.find('input[type="password"]').exists()).toBe(true)
    expect(wrapper.text()).not.toContain('اكتشف منتجاتنا')
  })

  it('switches pages when the hash changes', async () => {
    window.location.hash = ''
    const wrapper = mount(App)
    await flushPromises()

    window.location.hash = '#/admin'
    window.dispatchEvent(new HashChangeEvent('hashchange'))
    await flushPromises()

    expect(wrapper.find('input[type="password"]').exists()).toBe(true)

    window.location.hash = '#/'
    window.dispatchEvent(new HashChangeEvent('hashchange'))
    await flushPromises()

    expect(wrapper.text()).toContain('اكتشف منتجاتنا')
  })
})
