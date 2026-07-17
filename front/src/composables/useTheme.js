import { ref, watch } from 'vue'

const isDark = ref(false)
const stored = localStorage.getItem('theme')

if (stored !== null) {
  isDark.value = stored === 'dark'
} else {
  isDark.value = window.matchMedia('(prefers-color-scheme: dark)').matches
}

document.documentElement.classList.toggle('dark', isDark.value);

watch(isDark, (val) => {
  localStorage.setItem('theme', val ? 'dark' : 'light')
  document.documentElement.classList.toggle('dark', val)
})

export function useTheme() {
  return { isDark }
}
