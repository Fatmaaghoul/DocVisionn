<template>
  <div class="admin-container">
    <div class="card">
      <div class="card-content">
        <div class="header">
          <div class="title-section">
            <h2>Gestion des Documents</h2>
          </div>
          <div class="search-container">
            <div class="search-box">
              <input
                type="text"
                class="search-input"
                placeholder="Rechercher par nom, description, contenu texte, objets ou utilisateur..."
                v-model="searchQuery"
              />
              <button class="search-button">
                <i class="bi bi-search"></i>
              </button>
            </div>
          </div>
          <div class="user-filter-wrapper">
            <button class="filter-btn user-filter-btn" @click="toggleUserFilter">
              <i class="bi bi-person"></i>
              {{ selectedUserText }}
            </button>
            <div v-if="showUserFilter" class="user-filter-dropdown">
              <select v-model="selectedUserId" class="user-select">
                <option value="">Tous les utilisateurs</option>
                <option v-for="user in nonAdminUsers" :key="user.id" :value="user.id">
                  {{ user.userName || user.email }}
                </option>
              </select>
              <button v-if="selectedUserId" class="clear-user-btn" @click="clearUserFilter">
                Effacer le filtre
              </button>
            </div>
          </div>

          <div class="filter-container">
            <div class="date-filter-wrapper">
              <button
                class="filter-btn date-filter-btn"
                @click="toggleDatePicker"
              >
                <i class="bi bi-calendar"></i>
                {{ dateRangeText }}
              </button>
              <div v-if="showDatePicker" class="date-picker-dropdown">
                <div class="date-input">
                  <label>Date de début</label>
                  <input
                    type="date"
                    v-model="startDate"
                  />
                </div>
                <div class="date-input">
                  <label>Date de fin</label>
                  <input
                    type="date"
                    v-model="endDate"
                  />
                </div>
                <button
                  v-if="startDate || endDate"
                  class="clear-date-btn"
                  @click="clearDateFilter"
                >
                  Effacer le filtre de date
                </button>
              </div>
            </div>
          </div>

          <div class="action-buttons-container">
            <button @click="showAddModal = true" class="button primary">
              <i class="bi bi-plus-lg"></i> Ajouter un document
            </button>
            <div v-if="selectedDocuments.length > 0" class="selection-actions">
              <span class="selection-count">{{ selectedDocuments.length }} sélectionné(s)</span>
              <button @click="downloadSelectedDocuments()" class="button secondary" title="Télécharger les documents sélectionnés">
                <i class="bi bi-download"></i>
              </button>
              <button @click="deleteSelectedDocuments()" class="button danger" title="Supprimer les documents sélectionnés">
                <i class="bi bi-trash"></i>
              </button>
            </div>
          </div>
        </div>

        <!-- État de chargement -->
        <div v-if="loading" class="loading">
          <div class="spinner"></div>
          <p class="loading-text">Chargement des documents...</p>
        </div>

        <!-- Tableau des documents -->
        <div v-else class="table-wrapper">
          <table class="documents-table">
            <thead>
              <tr>
                <th class="checkbox-column">
                  <div class="checkbox-wrapper">
                    <input
                      type="checkbox"
                      id="select-all"
                      :checked="selectAll"
                      @change="toggleSelectAll"
                    />
                    <label for="select-all"></label>
                  </div>
                </th>
                <th>Document</th>
                <th>Utilisateur</th>
                <th>Description</th>
                <th>Ajouté le</th>
                <th>Analysé</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="document in filteredDocuments"
                :key="document.id"
                :class="{ 'selected-row': isSelected(document.id) }"
              >
                <td class="checkbox-column">
                  <div class="checkbox-wrapper">
                    <input
                      type="checkbox"
                      :id="'doc-' + document.id"
                      :checked="isSelected(document.id)"
                      @change="toggleSelectDocument(document.id)"
                    />
                    <label :for="'doc-' + document.id"></label>
                  </div>
                </td>
                <td>
                  <div class="document-name">
                    📄
                    <span>{{ document.name }}</span>
                  </div>
                </td>
                <td>
                  <div v-if="document.user" class="user-info">
                    {{ document.user.userName || document.user.email }}
                  </div>
                  <div v-else class="loading-info">Chargement de l'utilisateur...</div>
                </td>

                <td>
                  <div class="description">
                    <p>{{ document.description || 'Aucune description' }}</p>
                  </div>
                </td>

                <td>{{ formatDate(document.uploadDate) }}</td>
                <td class="analysis-status">
                  <div class="analysis-indicator">
                    <div class="indicator-icon" :class="{ 'analysed': document.isAnalysed }">
                      <i class="bi" :class="document.isAnalysed ? 'bi-check-circle-fill' : 'bi-x-circle'"></i>
                    </div>
                    <span class="status-text" :class="{ 'analysed': document.isAnalysed }">
                      {{ document.isAnalysed ? '' : '' }}
                    </span>
                  </div>
                </td>
                <td>
                  <div class="action-buttons">
                    <button class="btn-view" @click="viewDocument(document)" title="Voir">
                      <i class="bi bi-eye"></i>
                    </button>
                    <button class="btn-edit" @click="editDocument(document)" title="Modifier">
                      <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn-delete" @click="deleteDocument(document)" title="Supprimer">
                      <i class="bi bi-trash"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="filteredDocuments.length === 0">
                <td colspan="7" class="empty-state">
                  <div class="empty-message">
                    <i class="bi bi-folder-plus"></i>
                    <span>Aucun document trouvé</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modal Ajouter un document -->
    <div v-if="showAddModal" class="modal-overlay" @click="closeAddModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Ajouter un nouveau document</h2>
          <button class="close-btn" @click="closeAddModal">
            ❌
          </button>
        </div>

        <div class="modal-body">
          <div v-if="serverErrors.length" class="server-errors">
            <p v-for="(error, index) in serverErrors" :key="index" class="error-message">{{ error }}</p>
          </div>

          <div v-if="selectedFiles.length" class="form-fields">
            <div class="document-form" v-for="(file, index) in selectedFiles" :key="index">
              <h3 class="document-form-title">Document {{ index + 1 }}: {{ file.name }}</h3>

              <div class="form-group">
                <label>Nom du document</label>
                <input
                  type="text"
                  v-model="newDocuments[index].name"
                  class="form-input"
                  :class="{ 'error': !newDocuments[index].name.trim() && formSubmitted }"
                  :placeholder="'Nom pour ' + file.name"
                />
                <span v-if="!newDocuments[index].name.trim() && formSubmitted" class="error-message">
                  Le nom du document est requis.
                </span>
              </div>

              <div class="form-group">
                <label>Description <span class="description-info">(pré-remplie)</span></label>
                <textarea
                  v-model="newDocuments[index].description"
                  class="form-textarea"
                  rows="3"
                ></textarea>
              </div>
            </div>
          </div>

          <div class="form-group">
            <label>Sélectionner un utilisateur</label>
            <select
              v-model="newDocument.userId"
              class="form-input"
              :class="{ 'error': (formSubmitted && !newDocument.userId) || serverErrors.some(e => e.toLowerCase().includes('userid')) }"
            >
              <option value="" disabled>Sélectionnez un utilisateur</option>
              <option v-for="user in nonAdminUsers" :key="user.id" :value="user.id">
                {{ user.userName || user.email }}
              </option>
            </select>
            <span v-if="formSubmitted && !newDocument.userId" class="error-message">L'utilisateur est requis</span>
          </div>

          <div class="form-group">
            <label>Fichiers PDF</label>
            <div
              class="file-upload"
              @dragover.prevent="handleDragOver"
              @dragleave.prevent="handleDragLeave"
              @drop.prevent="handleDrop"
              :class="{ 'dragging': isDragging, 'error': (formSubmitted && selectedFiles.length === 0) || serverErrors.some(e => e.toLowerCase().includes('file')) }"
              @click="triggerFileInput"
            >
              <input
                type="file"
                @change="handleFileChange"
                class="file-input"
                accept=".pdf"
                ref="fileInput"
                multiple
              />
              <div class="upload-placeholder" v-if="selectedFiles.length === 0">
                <i class="bi bi-cloud-upload"></i>
                <p>Glissez-déposez vos fichiers PDF ici ou cliquez pour parcourir</p>
                <button class="browse-btn">Parcourir les fichiers</button>
              </div>
              <div class="file-preview-container" v-else>
                <div class="file-preview" v-for="(file, index) in selectedFiles" :key="index">
                  <span class="file-name">{{ file.name }}</span>
                  <button class="remove-file" @click.stop="removeFile(index)">×</button>
                </div>
              </div>
            </div>
            <span v-if="formSubmitted && selectedFiles.length === 0" class="error-message">Veuillez sélectionner au moins un fichier PDF</span>
          </div>
        </div>

        <div class="modal-footer">
          <button class="cancel-btn" @click="closeAddModal">Annuler</button>
          <button
            class="submit-btn"
            @click="uploadDocument"
            :disabled="!isFormValid || uploading"
          >
            <span v-if="uploading" class="spinner"></span>
            <span v-else>Ajouter</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Modal Modifier/Voir le document -->
    <div v-if="selectedDocument" class="modal-overlay" @click="closeDocumentModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ isEditing ? 'Modifier le document' : 'Détails du document' }}</h2>
          <button class="close-btn" @click="closeDocumentModal">
            ❌
          </button>
        </div>

        <div class="modal-body">
          <div v-if="serverErrors.length && isEditing" class="server-errors">
            <p v-for="(error, index) in serverErrors" :key="index" class="error-message">{{ error }}</p>
          </div>

          <div class="document-header">
            <h3 class="document-title">
              📄
              <span v-if="!isEditing">{{ selectedDocument.name }}</span>
              <input
                v-else
                v-model="selectedDocument.name"
                class="input"
                placeholder="Nom du document"
                :class="{ 'error': (formSubmitted && !selectedDocument.name) || serverErrors.some(e => e.toLowerCase().includes('name')) }"
              />
            </h3>
          </div>

          <div class="form-group">
            <label>Description</label>
            <p v-if="!isEditing">{{ selectedDocument.description || 'Aucune description' }}</p>
            <textarea
              v-else
              v-model="selectedDocument.description"
              class="textarea"
              placeholder="Entrez la description"
              :class="{ 'error': (formSubmitted && !selectedDocument.description) || serverErrors.some(e => e.toLowerCase().includes('description')) }"
            ></textarea>
          </div>

          <div class="form-group" v-if="isEditing">
            <label>Sélectionner un utilisateur</label>
            <select
              v-model="selectedDocument.userId"
              class="form-input"
              :class="{ 'error': (formSubmitted && !selectedDocument.userId) || serverErrors.some(e => e.toLowerCase().includes('userid')) }"
            >
              <option value="" disabled>Sélectionnez un utilisateur</option>
              <option v-for="user in nonAdminUsers" :key="user.id" :value="user.id">
                {{ user.userName || user.email }}
              </option>
            </select>
            <span v-if="formSubmitted && !selectedDocument.userId" class="error-message">L'utilisateur est requis</span>
          </div>

          <div class="form-group">
            <label>Ajouté le</label>
            <p>{{ formatDate(selectedDocument.uploadDate) }}</p>
          </div>

          <div v-if="selectedDocument.user" class="form-group">
            <label>Ajouter par</label>
            <div class="user-details">
              <p>{{ selectedDocument.user.userName || selectedDocument.user.email }}</p>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <div class="modal-actions">
            <button
              v-if="!isEditing"
              class="button secondary"
              @click="downloadDocument(selectedDocument)"
            >
              Télécharger
            </button>

            <div v-if="isEditing" class="edit-actions">
              <button
                class="button secondary"
                @click="cancelEdit"
              >
                Annuler
              </button>
              <button
                class="button primary"
                @click="saveDocumentChanges"
              >
                Enregistrer les modifications
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Cookies from 'js-cookie'

const router = useRouter()
const documents = ref([])
const loading = ref(true)
const searchQuery = ref('')
const selectedDocument = ref(null)
const isEditing = ref(false)
const documentBeforeEdit = ref(null)
const showAddModal = ref(false)
const startDate = ref('')
const endDate = ref('')
const showDatePicker = ref(false)
const newDocument = ref({
  userId: ''
})
const selectedFiles = ref([])
const newDocuments = ref([])
const isDragging = ref(false)
const uploading = ref(false)
const users = ref([])
const formSubmitted = ref(false)
const serverErrors = ref([])
const showUserFilter = ref(false)
const selectedUserId = ref('')
const selectedDocuments = ref([])
const selectAll = ref(false)

// Computed property to filter non-admin users
const nonAdminUsers = computed(() => {
  return users.value.filter(user =>
    !user.roles?.some(r => r.toLowerCase() === 'admin'))
})

const selectedUserText = computed(() => {
  if (!selectedUserId.value) return 'Filtrer par utilisateur'
  const user = nonAdminUsers.value.find(u => u.id === selectedUserId.value)
  return user ? (user.userName || user.email) : 'Utilisateur inconnu'
})

// Fetch all documents with their users
const fetchDocuments = async () => {
  loading.value = true
  try {
    const token = Cookies.get('token')
    const response = await axios.get('api/admin/documents', {
      headers: { Authorization: `Bearer ${token}` }
    })
    const documentsWithUsers = await Promise.all(
      response.data.map(async (doc) => {
        const user = await fetchDocumentUser(doc.id)
        return { ...doc, user }
      })
    )
    documents.value = documentsWithUsers
  } catch (error) {
    console.error('Erreur lors de la récupération des documents:', error)
    alert('Échec du chargement des documents. Veuillez réessayer.')
  } finally {
    loading.value = false
  }
}

// Fetch all users
const fetchUsers = async () => {
  try {
    const token = Cookies.get('token')
    const response = await axios.get('api/users/all', {
      headers: { Authorization: `Bearer ${token}` }
    })
    users.value = response.data
    console.log('Utilisateurs récupérés:', users.value)
  } catch (error) {
    console.error('Erreur lors de la récupération des utilisateurs:', error)
    alert('Échec du chargement des utilisateurs. Veuillez réessayer.')
  }
}

// Fetch document user
const fetchDocumentUser = async (documentId) => {
  try {
    const token = Cookies.get('token')
    const response = await axios.get(`api/admin/documents/${documentId}/user`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    return response.data
  } catch (error) {
    console.error('Erreur lors de la récupération de l\'utilisateur du document:', error)
    return null
  }
}

// View document (redirect to fileUrl)
const viewDocument = (document) => {
  if (document.fileUrl) {
    window.open(document.fileUrl, '_blank')
  } else {
    alert('Lien du document non disponible.')
  }
}

const toggleUserFilter = () => {
  showUserFilter.value = !showUserFilter.value
  // Ferme le date picker si ouvert
  showDatePicker.value = false
}

const clearUserFilter = () => {
  selectedUserId.value = ''
}

// Edit document
const editDocument = (document) => {
  isEditing.value = true
  documentBeforeEdit.value = { ...document }
  selectedDocument.value = { ...document, userId: document.user?.id || '' }
  serverErrors.value = []
  formSubmitted.value = false
}

// Save document changes
const saveDocumentChanges = async () => {
  formSubmitted.value = true
  serverErrors.value = []

  if (!selectedDocument.value.name.trim() || !selectedDocument.value.description.trim() || !selectedDocument.value.userId) {
    alert('Veuillez remplir tous les champs requis.')
    return
  }

  try {
    const token = Cookies.get('token')
    const response = await axios.put(`api/admin/documents/${selectedDocument.value.id}`,
      {
        Name: selectedDocument.value.name.trim(),
        Description: selectedDocument.value.description.trim(),
        UserId: selectedDocument.value.userId
      },
      { headers: { Authorization: `Bearer ${token}` } }
    )

    if (response.data) {
      const index = documents.value.findIndex(d => d.id === selectedDocument.value.id)
      if (index !== -1) {
        const updatedUser = await fetchDocumentUser(response.data.id)
        documents.value[index] = {
          ...response.data,
          user: updatedUser
        }
      }
      alert('Document modifié avec succès !')
      isEditing.value = false
      closeDocumentModal()
    }
  } catch (error) {
    console.error('Erreur lors de la modification du document:', error)
    if (error.response?.data) {
      const errors = error.response.data
      if (errors.errors) {
        serverErrors.value = Object.entries(errors.errors)
          .map(([field, messages]) => `${field}: ${messages.join(', ')}`)
      } else if (errors.error) {
        serverErrors.value = [errors.error]
      } else {
        serverErrors.value = ['Une erreur inattendue s\'est produite.']
      }
    } else {
      serverErrors.value = ['Échec de la modification du document. Aucune réponse du serveur.']
    }
  }
}

// Cancel edit
const cancelEdit = () => {
  if (documentBeforeEdit.value) {
    selectedDocument.value = { ...documentBeforeEdit.value }
  }
  isEditing.value = false
  serverErrors.value = []
  formSubmitted.value = false
}

// Delete document
const deleteDocument = async (document) => {
  if (confirm(`Voulez-vous vraiment supprimer "${document.name}" ?`)) {
    try {
      const token = Cookies.get('token')
      await axios.delete(`api/admin/documents/${document.id}`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      alert('Document supprimé avec succès !')
      await fetchDocuments()
    } catch (error) {
      console.error('Erreur lors de la suppression du document:', error)
      alert('Échec de la suppression du document. Veuillez réessayer.')
    }
  }
}

// Download document
const downloadDocument = async (document) => {
  try {
    const token = Cookies.get('token')
    const response = await axios.get(`api/documents/${document.id}/download`, {
      headers: { Authorization: `Bearer ${token}` },
      responseType: 'blob'
    })

    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', document.name)
    document.body.appendChild(link)
    link.click()
    link.remove()
  } catch (error) {
    console.error('Erreur lors du téléchargement du document:', error)
    alert('Échec du téléchargement du document. Veuillez réessayer.')
  }
}

// Format date
const formatDate = (date) => {
  if (!date) return 'N/A'
  const uploadDate = new Date(date)
  return uploadDate.toLocaleDateString('fr-FR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Filter documents based on search query and date range
const filteredDocuments = computed(() => {
  let filtered = documents.value

  // Filter by search query
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(doc =>
      // Search in document name
      doc.name.toLowerCase().includes(query) ||
      // Search in document description
      (doc.description && doc.description.toLowerCase().includes(query)) ||
      // Search in document text content
      (doc.text && doc.text.toLowerCase().includes(query)) ||
      // Search in objects
      (doc.allObjects && doc.allObjects.some(obj => 
        obj.toLowerCase().includes(query)
      )) ||
      // Search in user name
      (doc.user && doc.user.userName && doc.user.userName.toLowerCase().includes(query)) ||
      (doc.user && doc.user.email && doc.user.email.toLowerCase().includes(query))
    )
  }

  // Filter by date range
  if (startDate.value || endDate.value) {
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
        const start = startDate.value
          ? new Date(startDate.value).setHours(0, 0, 0, 0)
          : -Infinity
        const end = endDate.value
          ? new Date(endDate.value).setHours(23, 59, 59, 999)
          : Infinity
        return docDateNormalized >= start && docDateNormalized <= end
      } catch (error) {
        console.error(`Error processing uploadDate for document ${doc.id}:`, error)
        return false
      }
    })
  }
  
  // Filtre par utilisateur
  if (selectedUserId.value) {
    filtered = filtered.filter(doc => 
      doc.user && doc.user.id === selectedUserId.value
    )
  }

  return filtered
})

// Add Document Modal Logic
const isFormValid = computed(() => {
  if (selectedFiles.value.length === 0 || !newDocument.value.userId) {
    return false
  }

  // Check if all documents have names
  return newDocuments.value.every(doc => doc.name && doc.name.trim() !== '')
})

const triggerFileInput = () => {
  document.querySelector('.file-input').click()
}

const handleDragOver = () => {
  isDragging.value = true
}

const handleDragLeave = () => {
  isDragging.value = false
}

const handleDrop = (event) => {
  isDragging.value = false
  const files = Array.from(event.dataTransfer.files)
  handleFiles(files)
}

const handleFileChange = (event) => {
  const files = Array.from(event.target.files)
  handleFiles(files)
}

const handleFiles = (files) => {
  const validFiles = files.filter(file => file.type === 'application/pdf' && file.size <= 10 * 1024 * 1024)

  if (validFiles.length !== files.length) {
    alert('Seuls les fichiers PDF (max 10 Mo) sont autorisés.')
  }

  if (validFiles.length > 0) {
    selectedFiles.value = [...selectedFiles.value, ...validFiles]

    // Create document entries for each file
    const newEntries = validFiles.map(file => {
      const fileName = file.name.replace(/\.[^/.]+$/, '') // Remove file extension
      const currentDate = new Date().toLocaleDateString('fr-FR')

      // Create a default description based on the file name and date
      const defaultDescription = `Pas de description .`

      return {
        name: fileName,
        description: defaultDescription
      }  
    })

    newDocuments.value = [...newDocuments.value, ...newEntries]
  }
}

const removeFile = (index) => {
  selectedFiles.value.splice(index, 1)
  newDocuments.value.splice(index, 1)

  if (selectedFiles.value.length === 0 && document.querySelector('.file-input')) {
    document.querySelector('.file-input').value = ''
  }
}

const uploadDocument = async () => {
  formSubmitted.value = true
  serverErrors.value = []

  if (!isFormValid.value) {
    alert('Veuillez remplir tous les champs requis et sélectionner au moins un fichier.')
    return
  }

  uploading.value = true
  let successCount = 0
  let errorCount = 0

  try {
    const token = Cookies.get('token')

    // Upload each file individually
    for (let i = 0; i < selectedFiles.value.length; i++) {
      const file = selectedFiles.value[i]
      const docName = newDocuments.value[i].name

      const formData = new FormData()
      formData.append('File', file)
      formData.append('Name', docName.trim())
      formData.append('Description', newDocuments.value[i].description || '')
      formData.append('UserId', newDocument.value.userId)

      try {
        await axios.post('api/admin/documents', formData, {
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'multipart/form-data'
          }
        })
        successCount++
      } catch (error) {
        console.error(`Erreur lors du téléversement du document ${docName}:`, error)
        errorCount++

        if (error.response?.data) {
          const errors = error.response.data
          if (errors.errors) {
            serverErrors.value.push(`Erreur pour ${docName}: ${Object.values(errors.errors).flat().join(', ')}`)
          } else if (errors.error) {
            serverErrors.value.push(`Erreur pour ${docName}: ${errors.error}`)
          }
        }
      }
    }

    if (successCount > 0) {
      if (errorCount > 0) {
        alert(`${successCount} document(s) téléversé(s) avec succès. ${errorCount} échec(s).`)
      } else {
        alert(`${successCount} document(s) téléversé(s) avec succès !`)
      }

      closeAddModal()
      await fetchDocuments()
    } else {
      alert('Échec du téléversement de tous les documents.')
    }
  } catch (error) {
    console.error('Erreur générale lors du téléversement:', error)
    serverErrors.value.push('Une erreur inattendue s\'est produite.')
  } finally {
    uploading.value = false
  }
}

const closeAddModal = () => {
  showAddModal.value = false
  newDocument.value = { userId: '' }
  selectedFiles.value = []
  newDocuments.value = []
  isDragging.value = false
  formSubmitted.value = false
  serverErrors.value = []
}

const closeDocumentModal = () => {
  selectedDocument.value = null
  isEditing.value = false
  documentBeforeEdit.value = null
  serverErrors.value = []
  formSubmitted.value = false
}

// Date filter methods
const dateRangeText = computed(() => {
  if (!startDate.value && !endDate.value) return 'Filtrer par date'
  if (startDate.value === endDate.value && startDate.value)
    return new Date(startDate.value).toLocaleDateString('fr-FR')
  return `${startDate.value ? new Date(startDate.value).toLocaleDateString('fr-FR') : ''} - ${endDate.value ? new Date(endDate.value).toLocaleDateString('fr-FR') : ''}`
})

const toggleDatePicker = () => {
  showDatePicker.value = !showDatePicker.value
}

const clearDateFilter = () => {
  startDate.value = ''
  endDate.value = ''
}

// Méthodes pour la sélection multiple
const toggleSelectAll = () => {
  selectAll.value = !selectAll.value
  if (selectAll.value) {
    selectedDocuments.value = filteredDocuments.value.map(doc => doc.id)
  } else {
    selectedDocuments.value = []
  }
}

const toggleSelectDocument = (docId) => {
  const index = selectedDocuments.value.indexOf(docId)
  if (index === -1) {
    selectedDocuments.value.push(docId)
  } else {
    selectedDocuments.value.splice(index, 1)
  }

  // Mettre à jour l'état de selectAll
  selectAll.value = filteredDocuments.value.length > 0 &&
                   selectedDocuments.value.length === filteredDocuments.value.length
}

const isSelected = (docId) => {
  return selectedDocuments.value.includes(docId)
}

async function deleteSelectedDocuments() {
  if (selectedDocuments.value.length === 0) return

  if (!confirm(`Confirmez la suppression de ${selectedDocuments.value.length} document(s) ?`)) return

  try {
    const token = Cookies.get('token')
    let successCount = 0

    for (const docId of selectedDocuments.value) {
      try {
        await axios.delete(`api/admin/documents/${docId}`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        successCount++
      } catch (error) {
        console.error(`Erreur lors de la suppression du document ${docId}:`, error)
      }
    }

    // Mettre à jour la liste des documents
    await fetchDocuments()
    selectedDocuments.value = []
    selectAll.value = false

    alert(`${successCount} document(s) supprimé(s) avec succès`)
  } catch (error) {
    console.error('Erreur lors de la suppression des documents:', error)
    alert('Échec de la suppression des documents')
  }
}

async function downloadSelectedDocuments() {
  if (selectedDocuments.value.length === 0) return

  // Télécharger chaque document sélectionné
  for (const docId of selectedDocuments.value) {
    const doc = documents.value.find(d => d.id === docId)
    if (doc) {
      await downloadDocument(doc)
    }
  }
}

onMounted(() => {
  fetchDocuments()
  fetchUsers()

  // Close date picker when clicking outside
  document.addEventListener('click', (e) => {
    const datePicker = document.querySelector('.date-filter-wrapper')
    const datePickerBtn = document.querySelector('.date-filter-btn')
    const userFilter = document.querySelector('.user-filter-wrapper')
    const userFilterBtn = document.querySelector('.user-filter-btn')
    if (datePicker && !datePicker.contains(e.target) && datePickerBtn && !datePickerBtn.contains(e.target)) {
      showDatePicker.value = false
    }
    if (userFilter && !userFilter.contains(e.target) && userFilterBtn && !userFilterBtn.contains(e.target)) {
      showUserFilter.value = false
    }
  })
})
</script>

<style scoped>
.admin-container {
  padding: 2rem;
  max-width: 100%;
  margin: 0 auto;
}

.card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.card-content {
  padding: 1.5rem;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  flex-wrap: wrap;
  gap: 1rem;
}

.title-section h2 {
  margin: 0;
  font-size: 1.5rem;
  color: #2c3e50;
  display: flex;
  align-items: center;
}

.title-section p {
  margin: 0.25rem 0 0;
  color: #6c757d;
  font-size: 0.9rem;
}

.search-container {
  flex: 1 1 300px;
  max-width: 400px;
}

.filter-container {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.search-box {
  display: flex;
  border: 1px solid #ddd;
  border-radius: 4px;
  overflow: hidden;
}

.date-filter-wrapper {
  position: relative;
}

.filter-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: white;
  color: #666;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-btn i {
  font-size: 1rem;
}

.filter-btn:hover {
  background: #f8f9fa;
  border-color: #0d6efd;
  color: #0d6efd;
}

.date-filter-btn {
  min-width: 150px;
}

.date-picker-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  background: white;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 1rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  z-index: 1000;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-top: 0.5rem;
  min-width: 250px;
}

.date-input {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.date-input label {
  font-size: 0.85rem;
  color: #666;
}
.date-input input {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.9rem;
}

.date-input input:focus {
  outline: none;
  border-color: #0d6efd;
}

.clear-date-btn {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background: #f8f9fa;
  color: #666;
  font-size: 0.9rem;
  cursor: pointer;
  text-align: center;
}

.clear-date-btn:hover {
  background: #e9ecef;
  border-color: #0d6efd;
  color: #0d6efd;
}

.search-input {
  flex: 1;
  padding: 0.5rem;
  border: none;
  outline: none;
}

.search-button {
  padding: 0.5rem 1rem;
  background: none;
  border: none;
  color: #1976d2;
  border-left: 1px solid #ddd;
  cursor: pointer;
}

.search-button:hover {
  background: #f5f5f5;
}

.user-filter-wrapper {
  position: relative;
}

.user-filter-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  background: white;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 1rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  z-index: 1000;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-top: 0.5rem;
  min-width: 250px;
}

.user-select {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.9rem;
  width: 100%;
}

.clear-user-btn {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background: #f8f9fa;
  color: #666;
  font-size: 0.9rem;
  cursor: pointer;
  text-align: center;
}

.clear-user-btn:hover {
  background: #e9ecef;
  border-color: #0d6efd;
  color: #0d6efd;
}

.action-buttons-container {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.selection-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: #f8f9fa;
  padding: 0.5rem;
  border-radius: 4px;
  border: 1px solid #e9ecef;
}

.selection-count {
  font-size: 0.9rem;
  color: #495057;
  margin-right: 0.5rem;
}

.loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 3rem;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #f3f3f3;
  border-top: 3px solid #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.loading-text {
  margin-top: 1rem;
  color: #666;
}

.table-wrapper {
  width: 100%;
  overflow-x: hidden;
}

.documents-table {
  width: 100%;
  border-collapse: collapse;
  table-layout: auto;
}

.documents-table th,
.documents-table td {
  padding: 0.8rem;
  text-align: left;
  border-bottom: 1px solid #eee;
  vertical-align: middle;
  word-break: break-word;
}

.checkbox-column {
  width: 40px;
  text-align: center;
}

.checkbox-wrapper {
  position: relative;
  display: inline-block;
  width: 20px;
  height: 20px;
  margin: 0 auto;
}

.checkbox-wrapper input[type="checkbox"] {
  opacity: 0;
  position: absolute;
  width: 100%;
  height: 100%;
  cursor: pointer;
  z-index: 2;
  margin: 0;
}

.checkbox-wrapper label {
  position: absolute;
  top: 0;
  left: 0;
  width: 20px;
  height: 20px;
  background: white;
  border: 2px solid #b0b0b0;
  border-radius: 5px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.checkbox-wrapper input[type="checkbox"]:checked + label {
  background: #1976d2;
  border-color: #1976d2;
  animation: checkAnim 0.2s ease;
}

.checkbox-wrapper input[type="checkbox"]:checked + label:after {
  content: '';
  position: absolute;
  left: 6px;
  top: 2px;
  width: 5px;
  height: 10px;
  border: solid white;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

.checkbox-wrapper input[type="checkbox"]:focus + label {
  box-shadow: 0 0 0 3px rgba(25, 118, 210, 0.2);
}

@keyframes checkAnim {
  0% { transform: scale(1); }
  50% { transform: scale(1.1); }
  100% { transform: scale(1); }
}

.selected-row {
  background-color: #e3f2fd;
}

.selected-row td {
  background-color: #e3f2fd !important;
}

.selected-row:hover {
  background-color: #bbdefb;
}

.documents-table th {
  font-weight: 600;
  color: #2c3e50;
  background: #f8f9fa;
}

.documents-table tr:hover td {
  background-color: #f5f5f5;
}

.documents-table td {
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.document-name {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.description {
  max-width: 250px;
}

.description p {
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.analysis-status {
  text-align: center;
}

.analysis-indicator {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.indicator-icon {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.indicator-icon i {
  font-size: 18px;
}

.indicator-icon .bi-check-circle-fill {
  color: #4caf50;
}

.indicator-icon .bi-x-circle {
  color: #f44336;
}

.status-text {
  font-size: 0.85rem;
  color: #666;
}

.status-text.analysed {
  color: #4caf50;
}

.empty-state {
  text-align: center;
  padding: 3rem;
}

.empty-message {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  color: #666;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #eee;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
}

.modal-body {
  padding: 1.5rem;
}

.modal-footer {
  padding: 1rem 1.5rem;
  border-top: 1px solid #eee;
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-input,
.form-textarea,
.input,
.textarea {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.form-input.error,
.form-textarea.error,
.file-upload.error {
  border-color: #e74c3c;
}

.error-message {
  color: #e74c3c;
  font-size: 0.85rem;
  margin-top: 0.25rem;
  display: block;
}

.form-textarea,
.textarea {
  resize: vertical;
  min-height: 100px;
}

.file-upload {
  border: 2px dashed #ddd;
  border-radius: 4px;
  padding: 1.5rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s;
}

.file-upload.dragging {
  border-color: #3498db;
  background: #f8f9fa;
}

.file-input {
  display: none;
}

.upload-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.upload-placeholder i {
  font-size: 2rem;
  color: #666;
}

.browse-btn {
  margin-top: 0.5rem;
  padding: 0.5rem 1rem;
  background: #f8f9fa;
  border: 1px solid #ddd;
  border-radius: 4px;
  cursor: pointer;
}

.browse-btn:hover {
  background: #e8ecef;
}

.file-preview-container {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  max-height: 200px;
  overflow-y: auto;
  width: 100%;
}

.file-preview {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.5rem;
  background: #f8f9fa;
  border-radius: 4px;
  margin-bottom: 0.5rem;
}

.remove-file {
  background: none;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
  color: #e74c3c;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.action-buttons button {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-edit {
  background: #e3f2fd;
  color: #1976d2;
}

.btn-edit:hover {
  background: #bbdefb;
}

.btn-view {
  background: #fff8e1;
  color: #ffa000;
}

.btn-view:hover {
  background: #ffecb3;
}

.btn-delete {
  background: #ffebee;
  color: #d32f2f;
}

.btn-delete:hover {
  background: #ffcdd2;
}

.cancel-btn,
.submit-btn,
.button {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
}

.cancel-btn,
.button.secondary {
  background: #f8f9fa;
  color: #2c3e50;
}

.submit-btn,
.button.primary {
  background: #3498db;
  color: white;
}

.button.secondary {
  background: #6c757d;
  color: white;
}

.button.secondary:hover {
  background: #5a6268;
}

.button.danger {
  background: #dc3545;
  color: white;
}

.button.danger:hover {
  background: #c82333;
}

.submit-btn:disabled {
  background: #95a5a6;
  cursor: not-allowed;
}

.document-header {
  background: #f8f9fa;
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
}

.document-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.user-details {
  background: #f8f9fa;
  padding: 1rem;
  border-radius: 4px;
}

.user-details p {
  margin: 0.5rem 0;
}

.modal-actions {
  display: flex;
  justify-content: space-between;
  width: 100%;
}

.edit-actions {
  display: flex;
  gap: 0.5rem;
}

.server-errors {
  background: #ffe6e6;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.server-errors .error-message {
  margin: 0.25rem 0;
}

.form-fields {
  margin-bottom: 1.5rem;
  max-height: 400px;
  overflow-y: auto;
  padding-right: 10px;
}

.document-form {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 15px;
  margin-bottom: 20px;
  border-left: 4px solid #3498db;
}

.document-form-title {
  font-size: 1.1rem;
  margin-top: 0;
  margin-bottom: 15px;
  color: #2c3e50;
  display: flex;
  align-items: center;
  gap: 8px;
}

.document-form-title::before {
  content: "📄";
  font-size: 1.2rem;
}

.description-info {
  font-size: 0.8rem;
  color: #6c757d;
  font-weight: normal;
  margin-left: 5px;
}

.help-text {
  display: block;
  font-size: 0.8rem;
  color: #6c757d;
  margin-top: 5px;
  font-style: italic;
}

@media (max-width: 768px) {
  .admin-container {
    padding: 1rem;
  }

  .header {
    flex-direction: column;
    gap: 1rem;
  }

  .search-container {
    width: 100%;
    max-width: none;
  }

  .documents-table th,
  .documents-table td {
    padding: 0.5rem;
    font-size: 0.9rem;
  }

  .description {
    max-width: 150px;
  }

  .modal-content {
    width: 95%;
  }

  .modal-actions {
    flex-direction: column;
    gap: 1rem;
  }

  .edit-actions {
    width: 100%;
  }

  .button {
    width: 100%;
    justify-content: center;
  }
}
</style>



