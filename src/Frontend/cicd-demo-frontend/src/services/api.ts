import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080',
  headers: {
    Accept: 'application/json',
  },
  timeout: 10000,
})

export default api
