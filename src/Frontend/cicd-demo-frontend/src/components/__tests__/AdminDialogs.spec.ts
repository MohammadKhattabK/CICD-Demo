import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import AdminLoginForm from '../AdminLoginForm.vue'
import ConfirmDeleteModal from '../ConfirmDeleteModal.vue'
import type { Product } from '../../types/Product'

const product: Product = {
  id: 3,
  name: 'Wireless Mouse',
  description: 'Comfortable mouse',
  price: 59.5,
  category: 'Electronics',
  stock: 4,
  isActive: true,
  imageUrl: '/images/mouse.jpg',
}

describe('AdminLoginForm', () => {
  it('submits the trimmed key', async () => {
    const wrapper = mount(AdminLoginForm, { props: { busy: false, error: '' } })

    await wrapper.find('input[type="password"]').setValue('  secret-key  ')
    await wrapper.find('form').trigger('submit')

    expect(wrapper.emitted('submit')).toEqual([['secret-key']])
  })

  it('does not submit an empty key and shows errors', async () => {
    const wrapper = mount(AdminLoginForm, { props: { busy: false, error: 'مفتاح خاطئ' } })

    await wrapper.find('form').trigger('submit')

    expect(wrapper.emitted('submit')).toBeUndefined()
    expect(wrapper.find('[role="alert"]').text()).toBe('مفتاح خاطئ')
  })
})

describe('ConfirmDeleteModal', () => {
  it('names the product and emits confirm and cancel', async () => {
    const wrapper = mount(ConfirmDeleteModal, { props: { product, busy: false, error: '' } })

    expect(wrapper.text()).toContain('Wireless Mouse')

    const [cancel, confirm] = wrapper.findAll('button')
    await confirm!.trigger('click')
    await cancel!.trigger('click')

    expect(wrapper.emitted('confirm')).toHaveLength(1)
    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })

  it('disables deleting while busy', () => {
    const wrapper = mount(ConfirmDeleteModal, { props: { product, busy: true, error: '' } })

    expect(wrapper.findAll('button')[1]!.attributes('disabled')).toBeDefined()
  })
})
