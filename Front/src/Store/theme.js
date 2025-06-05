import { defineStore } from 'pinia'

export const useThemeStore = defineStore('theme', {
  state: () => ({
    isDark: false
  }),
  actions: {
    toggleTheme() {
      this.isDark = !this.isDark
      // Sauvegarder la préférence dans le localStorage
      localStorage.setItem('theme', this.isDark ? 'dark' : 'light')
      // Appliquer la classe au body
      document.body.classList.toggle('dark-theme', this.isDark)
    },
    initTheme() {
      // Récupérer la préférence du localStorage ou utiliser la préférence système
      const savedTheme = localStorage.getItem('theme')
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
      
      this.isDark = savedTheme ? savedTheme === 'dark' : prefersDark
      document.body.classList.toggle('dark-theme', this.isDark)
    }
  }
}) 