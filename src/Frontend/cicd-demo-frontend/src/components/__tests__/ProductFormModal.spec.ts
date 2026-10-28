import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ProductFormModal from '../ProductFormModal.vue'
import type { Product } from '../../types/Product'

const existing: Product = {
  id: 7,
  name: 'Desk Lamp',
  description: 'Adjustable lamp',
  price: 89,
  category: 'Home',
  stock: 8,
  isActive: true,
  imageUrl: '/images/desk-lamp.jpg',
}

const baseProps = { product: null, categories: ['Home', 'Electronics'], busy: false, error: '' }

describe('ProductFormModal', () => {
  it('shows validation errors and blocks saving an empty form', async () => {
    const wrapper = mount(ProductFormModal, { props: baseProps })

    expect(wrapper.find('button[type="submit"]').attributes('disabled')).toBeDefined()

    await wrapper.find('form').trigger('submit')

    expect(wrapper.emitted('save')).toBeUndefined()
    expect(wrapper.text()).toContain('الاسم مطلوب.')
    expect(wrapper.text()).toContain('التصنيف مطلوب.')
  })

  it('emits a trimmed product when creating', async () => {
    const wrapper = mount(ProductFormModal, { props: baseProps })

    await wrapper.find('input[name="name"]').setValue('  Standing Desk  ')
    await wrapper.find('input[name="category"]').setValue('Home')
    await wrapper.find('input[name="price"]').setValue('399.9')
    await wrapper.find('input[name="stock"]').setValue('5')
    await wrapper.find('form').trigger('submit')

    expect(wrapper.emitted('save')).toEqual([
      [
        {
          name: 'Standing Desk',
          description: '',
          price: 399.9,
          category: 'Home',
          stock: 5,
          imageUrl: '',
        },
      ],
    ])
  })

  it('prefills the fields when editing', () => {
    const wrapper = mount(ProductFormModal, { props: { ...baseProps, product: existing } })

    expect(wrapper.text()).toContain('تعديل المنتج')
    expect((wrapper.find('input[name="name"]').element as HTMLInputElement).value).toBe('Desk Lamp')
    expect((wrapper.find('input[name="stock"]').element as HTMLInputElement).value).toBe('8')
  })

  it('rejects a negative price', async () => {
    const wrapper = mount(ProductFormModal, { props: { ...baseProps, product: existing } })

    await wrapper.find('input[name="price"]').setValue('-1')

    expect(wrapper.text()).toContain('السعر يجب أن يكون')
    expect(wrapper.find('button[type="submit"]').attributes('disabled')).toBeDefined()
  })

  it('shows a server error and emits cancel', async () => {
    const wrapper = mount(ProductFormModal, { props: { ...baseProps, error: 'خطأ من الخادم' } })

    expect(wrapper.find('[role="alert"]').text()).toBe('خطأ من الخادم')

    await wrapper.find('button[type="button"]').trigger('click')

    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })
})
