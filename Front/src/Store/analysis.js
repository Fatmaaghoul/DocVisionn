import { defineStore } from 'pinia'

export const useDocumentStore = defineStore('document', {
  state: () => ({
    isAnalysing: false,
    isExtracting: false, // Nouvel état pour l'extraction
    taskType: '',
    taskItemName: ''
  }),
  actions: {
    startAnalysing(type = '', itemName = '') {
      this.isAnalysing = true
      this.taskType = type
      this.taskItemName = itemName
      
      // Stocker dans localStorage
      localStorage.setItem('isAnalysing', 'true')
      localStorage.setItem('taskType', type)
      localStorage.setItem('taskItemName', itemName)
      
      console.log(`Démarrage analyse: ${type} pour ${itemName}`)
    },
    stopAnalysing() {
      this.isAnalysing = false
      
      // Nettoyer localStorage
      localStorage.setItem('isAnalysing', 'false')
      localStorage.removeItem('taskType')
      localStorage.removeItem('taskItemName')
      
      console.log('Arrêt analyse')
    },
    startExtracting(fileName = '') {
      this.isExtracting = true
      this.taskItemName = fileName
      
      // Stocker dans localStorage
      localStorage.setItem('isExtracting', 'true')
      localStorage.setItem('taskItemName', fileName)
      
      console.log(`Démarrage extraction pour ${fileName}`)
    },
    stopExtracting() {
      this.isExtracting = false
      this.taskItemName = ''
      
      // Nettoyer localStorage
      localStorage.setItem('isExtracting', 'false')
      localStorage.removeItem('taskItemName')
      
      console.log('Arrêt extraction')
    }
  }
})



