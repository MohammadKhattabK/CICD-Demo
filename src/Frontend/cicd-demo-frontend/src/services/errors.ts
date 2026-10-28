import axios from 'axios'

/** Turns an API/network error into a message that is safe to show to the user. */
export function getErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) return 'حدث خطأ غير متوقع.'

  const status = error.response?.status

  if (!error.response) return 'تعذر الاتصال بالخادم. تحقق من الاتصال وحاول مجددًا.'
  if (status === 401) return 'مفتاح الإدارة غير صحيح.'
  if (status === 403) return 'عمليات التعديل معطلة على الخادم (لم يُضبط مفتاح الإدارة).'
  if (status === 404) return 'المنتج غير موجود، ربما حُذف مسبقًا.'

  if (status === 400) {
    const errors = error.response.data?.errors as Record<string, string[]> | undefined
    const messages = errors ? Object.values(errors).flat() : []
    if (messages.length) return messages.join(' ')
  }

  return 'تعذر إكمال العملية. حاول مرة أخرى.'
}

export function isUnauthorized(error: unknown): boolean {
  return axios.isAxiosError(error) && error.response?.status === 401
}
