<template>
  <div class="documents-container">
    <SearchFilter
      v-model:search-query="searchQuery"
      :current-filter="currentFilter"
      v-model:start-date="startDate"
      v-model:end-date="endDate"
      @update:filter="setFilter"
      @clear-search="clearSearch"
      @clear-date-filter="clearDateFilter"
    />
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Chargement des documents...</p>
    </div>
    <DocumentGrid
      v-else
      :documents="paginatedDocuments"
      @navigate-to-content="navigateToContentDocument"
      @download-document="downloadDocument"
      @delete-document="deleteDocument"
      @show-add-modal="showAddModal = true"
    />
    <div v-if="totalPages > 1" class="pagination-container">
      <button
        v-for="page in totalPages"
        :key="page"
        class="pagination-btn"
        :class="{ active: currentPage === page }"
        @click="setPage(page)"
      >
        {{ page }}
      </button>
    </div>
    <AddDocumentModal
      v-if="showAddModal"
      @close="closeModal"
      @upload-documents="uploadDocuments"
    />
  </div>
</template>

<script>
import axios from 'axios'
import Cookies from 'js-cookie'
import SearchFilter from './SearchFilter.vue'
import DocumentGrid from './DocumentGrid.vue'
import AddDocumentModal from './AddDocumentModal.vue'

export default {
  name: 'ListDocument',
  components: {
    SearchFilter,
    DocumentGrid,
    AddDocumentModal
  },
  data() {
    return {
      documents: [],
      loading: true,
      searchQuery: '',
      showAddModal: false,
      currentFilter: 'all',
      currentPage: 1,
      documentsPerPage: 10,
      startDate: '',
      endDate: ''
    }
  },
  computed: {
    filteredDocuments() {
      let filtered = this.documents

      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase()
        filtered = filtered.filter((doc) => {
          const name = doc.name ? doc.name.toLowerCase() : ''
          const description = doc.description ? doc.description.toLowerCase() : ''
          const text = doc.text ? doc.text.toLowerCase() : ''
          // Correction pour allObjects qui est un tableau de chaînes
          const objectsText = doc.allObjects ? 
            doc.allObjects.join(' ').toLowerCase() : ''
          
          return (
            name.includes(query) ||
            description.includes(query) ||
            text.includes(query) ||
            objectsText.includes(query)
          )
        })
      }

      if (this.currentFilter === 'traiter') {
        filtered = filtered.filter((doc) => doc.isAnalysed)
      } else if (this.currentFilter === 'non-traiter') {
        filtered = filtered.filter((doc) => !doc.isAnalysed)
      }

      if (this.startDate || this.endDate) {
        filtered = filtered.filter((doc) => {
          try {
            if (!doc.uploadDate) {
              console.warn(`Document ${doc.id} has no uploadDate`, doc)
              return false
            }
            const docDate = new Date(doc.uploadDate)
            if (isNaN(docDate.getTime())) {
              console.warn(`Invalid uploadDate for document ${doc.id}: ${doc.uploadDate}`)
              return false
            }
            const docDateNormalized = docDate.setHours(0, 0, 0, 0)
            const start = this.startDate
              ? new Date(this.startDate).setHours(0, 0, 0, 0)
              : -Infinity
            const end = this.endDate
              ? new Date(this.endDate).setHours(23, 59, 59, 999)
              : Infinity
            return docDateNormalized >= start && docDateNormalized <= end
          } catch (error) {
            console.error(`Error processing uploadDate for document ${doc.id}:`, error)
            return false
          }
        })
      }

      // Trier les documents par date (les plus récents en haut)
      filtered.sort((a, b) => {
        // Si un document n'a pas de date, le placer en bas
        if (!a.uploadDate) return 1
        if (!b.uploadDate) return -1

        // Convertir les dates en objets Date pour la comparaison
        const dateA = new Date(a.uploadDate)
        const dateB = new Date(b.uploadDate)

        // Vérifier si les dates sont valides
        if (isNaN(dateA.getTime())) return 1
        if (isNaN(dateB.getTime())) return -1

        // Trier par ordre décroissant (les plus récents en haut)
        return dateB - dateA
      })

      return filtered
    },
    paginatedDocuments() {
      const start = (this.currentPage - 1) * this.documentsPerPage
      const end = start + this.documentsPerPage
      return this.filteredDocuments.slice(start, end)
    },
    totalPages() {
      return Math.ceil(this.filteredDocuments.length / this.documentsPerPage)
    }
  },
  methods: {
    setFilter(filter) {
      this.currentFilter = filter
      this.currentPage = 1
    },
    setPage(page) {
      this.currentPage = page
    },
    async fetchDocuments() {
      this.loading = true
      try {
        const token = Cookies.get('token')
        const response = await axios.get('api/documents', {
          headers: {
            Authorization: `Bearer ${token}`
          }
        })
        this.documents = response.data
        // Log uploadDate format for debugging
        this.documents.forEach(doc => {
          if (doc.uploadDate) {
            console.log(`Document ${doc.id} uploadDate: ${doc.uploadDate}`)
          } else {
            console.warn(`Document ${doc.id} has no uploadDate`)
          }
        })
      } catch (error) {
        console.error('Erreur lors du chargement des documents:', error)
        this.showErrorAlert('Erreur lors du chargement des documents. Veuillez réessayer.')
      } finally {
        this.loading = false
      }
    },
    clearSearch() {
      this.searchQuery = ''
      this.currentPage = 1
    },
    clearDateFilter() {
      this.startDate = ''
      this.endDate = ''
      this.currentPage = 1
    },
    showSuccessAlert(message) {
      const alertDiv = document.createElement('div')
      alertDiv.className = 'custom-alert success'
      alertDiv.innerHTML = `
        <i class="bi bi-check-circle"></i>
        <span>${message}</span>
      `
      document.body.appendChild(alertDiv)
      setTimeout(() => {
        alertDiv.classList.add('show')
        setTimeout(() => {
          alertDiv.classList.remove('show')
          setTimeout(() => {
            document.body.removeChild(alertDiv)
          }, 300)
        }, 3000)
      }, 100)
    },
    showErrorAlert(message) {
      const alertDiv = document.createElement('div')
      alertDiv.className = 'custom-alert error'
      alertDiv.innerHTML = `
        <i class="bi bi-exclamation-circle"></i>
        <span>${message}</span>
      `
      document.body.appendChild(alertDiv)
      setTimeout(() => {
        alertDiv.classList.add('show')
        setTimeout(() => {
          alertDiv.classList.remove('show')
          setTimeout(() => {
            document.body.removeChild(alertDiv)
          }, 300)
        }, 3000)
      }, 100)
    },
    navigateToContentDocument(doc) {
      this.$router.push({ name: 'ContentDocument', params: { id: doc.id } })
    },
    async downloadDocument(doc) {
      try {
        const token = Cookies.get('token')
        const response = await axios({
          url: doc.fileUrl,
          method: 'GET',
          responseType: 'blob',
          headers: {
            Authorization: `Bearer ${token}`
          }
        })

        const url = window.URL.createObjectURL(new Blob([response.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', doc.name)
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        window.URL.revokeObjectURL(url)
      } catch (error) {
        console.error('Erreur lors du téléchargement:', error)
        this.showErrorAlert('Erreur lors du téléchargement du document. Veuillez réessayer.')
      }
    },
    async deleteDocument(documentId) {
      if (!confirm('Voulez-vous vraiment supprimer ce document ?')) return
      try {
        const token = Cookies.get('token')
        await axios.delete(`api/documents/${documentId}`, {
          headers: {
            Authorization: `Bearer ${token}`
          }
        })
        this.documents = this.documents.filter((doc) => doc.id !== documentId)
        if (this.paginatedDocuments.length === 0 && this.currentPage > 1) {
          this.currentPage--
        }
        this.showSuccessAlert('Document supprimé avec succès !')
      } catch (error) {
        console.error('Erreur lors de la suppression:', error)
        this.showErrorAlert('Erreur lors de la suppression du document. Veuillez réessayer.')
      }
    },
    closeModal() {
      this.showAddModal = false
    },
    async uploadDocuments(documents) {
      try {
        const token = Cookies.get('token')
        if (!token) {
          throw new Error('Jeton authentification non trouvé. Veuillez vous reconnecter.')
        }

        const uploadPromises = documents.map(async ({ file, name, description }) => {
          const formData = new FormData()
          formData.append('file', file)
          formData.append('name', name.trim())
          formData.append('description', description || 'Pas de description .')

          const response = await axios.post('api/documents/add', formData, {
            headers: {
              Authorization: `Bearer ${token}`,
              'Content-Type': 'multipart/form-data',
              Accept: 'application/json'
            }
          })
          return response.data
        })

        const newDocuments = await Promise.all(uploadPromises)
        this.documents = [...newDocuments, ...this.documents]
        this.currentPage = 1
        this.showSuccessAlert('Documents uploadés avec succès !')
        this.closeModal()
      } catch (error) {
        let errorMessage = 'Erreur lors de l upload des documents. Veuillez réessayer.'
        if (error.response && error.response.data) {
          const errorData = error.response.data
          if (errorData.errors) {
            errorMessage = Object.entries(errorData.errors)
              .map(([field, messages]) => `${field}: ${messages.join(', ')}`)
              .join('\n')
          } else if (errorData.title) {
            errorMessage = errorData.title
          } else if (typeof errorData === 'string') {
            errorMessage = errorData
          }
        } else if (error.message) {
          errorMessage = error.message
        }
        this.showErrorAlert(errorMessage)
      }
    }
  },
  mounted() {
    this.fetchDocuments()
  }
}
</script>

<style scoped>
.documents-container {
  max-width: 1200px;
  margin: 80px auto 0;
  padding: 1.5rem;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem;
  color: #6c757d;
  font-size: 0.9rem;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #f8f9fa;
  border-top: 3px solid #4e73df;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  margin-bottom: 0.75rem;
}

.pagination-container {
  display: flex;
  justify-content: center;
  gap: 0.5rem;
  margin-top: 1.5rem;
}

.pagination-btn {
  padding: 0.5rem 1rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: #f8f9fa;
  color: #2c3e50;
  font-size: 0.95rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.pagination-btn:hover {
  background: #e9ecef;
  border-color: #4e73df;
}

.pagination-btn.active {
  background: #4e73df;
  color: white;
  border-color: #4e73df;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.custom-alert {
  position: fixed;
  top: 20px;
  right: 20px;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  transform: translateX(120%);
  transition: transform 0.3s ease;
  z-index: 9999;
}

.custom-alert.show {
  transform: translateX(0);
}

.custom-alert.success {
  background: #e3f2fd;
  color: #1976d2;
}

.custom-alert.error {
  background: #ffebee;
  color: #d32f2f;
}

.custom-alert i {
  font-size: 1rem;
}

.custom-alert span {
  font-size: 0.85rem;
  font-weight: 500;
}

@media (max-width: 768px) {
  .documents-container {
    padding: 1rem;
    margin: 60px auto 0;
  }

  .pagination-container {
    flex-wrap: wrap;
    gap: 0.25rem;
  }

  .pagination-btn {
    padding: 0.5rem 0.75rem;
    font-size: 0.9rem;
  }
}

@media (max-width: 576px) {
  .documents-container {
    padding: 0.75rem;
  }

  .loading-container {
    font-size: 0.85rem;
  }

  .spinner {
    width: 24px;
    height: 24px;
    border-width: 2px;
  }
}
</style>



