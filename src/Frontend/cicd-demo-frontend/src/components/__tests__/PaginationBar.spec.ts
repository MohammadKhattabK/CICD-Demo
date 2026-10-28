import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import PaginationBar from '../PaginationBar.vue'

describe('PaginationBar', () => {
  it('is hidden when there is a single page', () => {
    const wrapper = mount(PaginationBar, { props: { page: 1, totalPages: 1 } })

    expect(wrapper.find('nav').exists()).toBe(false)
  })

  it('shows the current page and total', () => {
    const wrapper = mount(PaginationBar, { props: { page: 2, totalPages: 5 } })

    expect(wrapper.text()).toContain('صفحة 2 من 5')
  })

  it('disables previous on the first page and next on the last page', async () => {
    const wrapper = mount(PaginationBar, { props: { page: 1, totalPages: 3 } })
    const [previous, next] = wrapper.findAll('button')

    expect(previous!.attributes('disabled')).toBeDefined()
    expect(next!.attributes('disabled')).toBeUndefined()

    await wrapper.setProps({ page: 3 })

    expect(previous!.attributes('disabled')).toBeUndefined()
    expect(next!.attributes('disabled')).toBeDefined()
  })

  it('emits the new page when navigating', async () => {
    const wrapper = mount(PaginationBar, { props: { page: 2, totalPages: 3 } })
    const [previous, next] = wrapper.findAll('button')

    await next!.trigger('click')
    await previous!.trigger('click')

    expect(wrapper.emitted('update:page')).toEqual([[3], [1]])
  })
})
