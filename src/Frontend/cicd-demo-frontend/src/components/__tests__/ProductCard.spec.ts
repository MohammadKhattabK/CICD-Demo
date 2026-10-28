
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ProductCard from '../ProductCard.vue'
import type { Product } from '../../types/Product'

const product: Product = {
  id: 1,
  name: 'Organic Tomato Seeds',
  description: 'High-quality tomato seeds',
  price: 12.5,
  category: 'Seeds',
  stock: 25,
  isActive: true,
  imageUrl: 'https://example.com/tomato-seeds.jpg',
}

describe('ProductCard', () => {
  it('displays the product information', () => {
    const wrapper = mount(ProductCard, {
      props: { product },
    })

    expect(wrapper.text()).toContain('Organic Tomato Seeds')
    expect(wrapper.text()).toContain('High-quality tomato seeds')
    expect(wrapper.text()).toContain('Seeds')
    expect(wrapper.text()).toContain('$12.50')
  })

  it('displays the product image with the correct URL and alt text', () => {
    const wrapper = mount(ProductCard, {
      props: { product },
    })

    const image = wrapper.get('img')

    expect(image.attributes('src')).toBe(
      'https://example.com/tomato-seeds.jpg',
    )
    expect(image.attributes('alt')).toBe('Organic Tomato Seeds')
  })

  it('uses the local placeholder when the image URL is empty', () => {
    const wrapper = mount(ProductCard, {
      props: {
        product: { ...product, imageUrl: '' },
      },
    })

    expect(wrapper.get('img').attributes('src')).toContain(
      'images/product-placeholder.svg',
    )
  })

  it('displays the stock quantity when the product is in stock', () => {
    const wrapper = mount(ProductCard, {
      props: { product },
    })

    expect(wrapper.text()).toContain('25')
  })

  it('handles products with zero stock', () => {
    const wrapper = mount(ProductCard, {
      props: {
        product: { ...product, stock: 0 },
      },
    })

    expect(wrapper.find('.out-of-stock').exists()).toBe(true)
    expect(wrapper.find('.in-stock').exists()).toBe(false)
  })
})
