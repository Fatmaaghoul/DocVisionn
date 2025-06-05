import { defineStore } from 'pinia'

export const useModelStore = defineStore('model', {
  state: () => ({
    isDownloading: false,
    downloadProgress: 0,
    downloadStatus: null, // 'downloading', 'completed', 'error', 'cancelled'
    currentDownloadingModel: null,
    error: null
  }),

  actions: {
    startDownload(modelName) {
      this.isDownloading = true
      this.downloadProgress = 0
      this.downloadStatus = 'downloading'
      this.currentDownloadingModel = modelName
      this.error = null
    },

    updateProgress(progress) {
      this.downloadProgress = progress
    },

    completeDownload() {
      this.isDownloading = false
      this.downloadProgress = 0
      this.downloadStatus = 'completed'
      this.currentDownloadingModel = null
    },

    cancelDownload() {
      this.isDownloading = false
      this.downloadProgress = 0
      this.downloadStatus = 'cancelled'
      this.currentDownloadingModel = null
    },

    setError(error) {
      this.isDownloading = false
      this.downloadProgress = 0
      this.downloadStatus = 'error'
      this.currentDownloadingModel = null
      this.error = error
    },

    reset() {
      this.isDownloading = false
      this.downloadProgress = 0
      this.downloadStatus = null
      this.currentDownloadingModel = null
      this.error = null
    }
  }
}) 