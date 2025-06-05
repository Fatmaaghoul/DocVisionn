<template>
  <div class="admin-container">
    <div class="card">
      <div class="card-content">
        <div class="header">
          <h2 class="title">Mes Documents</h2>
          <div class="search-container">
            <div class="search-box">
              <input
                type="text"
                class="search-input"
                placeholder="Rechercher par nom, description, contenu texte ou objets..."
                v-model="searchQuery"
              />
              <button class="search-button">
                <i class="bi bi-search"></i>
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
                {{ dateRangeText() }}
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
                  <div class="document-name" @click="viewDocument(document)">
                    📄
                    <span>{{ document.name }}</span>
                  </div>
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
                    <button class="btn-view" @click="downloadDocument(document)" title="Télécharger">
                      <i class="bi bi-download"></i>
                    </button>
                    <button class="btn-delete" @click="deleteDocument(document.id)" title="Supprimer">
                      <i class="bi bi-trash"></i>
                    </button>
                    <button class="btn-view" @click="viewDocument(document)" title="Voir">
  <i class="bi bi-eye"></i>
</button>
                  </div>
                </td>
              </tr>
              <tr v-if="filteredDocuments.length === 0">
                <td colspan="4" class="empty-state">
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
          <button class="close-btn" @click="closeAddModal">❌</button>
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
  </div>
</template>

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

.action-buttons-container {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.action-buttons button {
  transition: all 0.2s ease;
  border: 1px solid transparent;
}

.action-buttons button:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
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
/* Nouveau style pour les icônes d'analyse */
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
  color: #4caf50; /* Vert pour indiquer que l'analyse est terminée */
}

.indicator-icon .bi-x-circle {
  color: #f44336; /* Rouge pour indiquer que l'analyse n'est pas faite */
}

.status-text {
  font-size: 0.85rem;
  color: #666;
}

.status-text.analysed {
  color: #4caf50;
}
.title {
  margin: 0;
  font-size: 1.5rem;
  color: #2c3e50;
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
.btn-view {
  background: #e3f2fd;
  color: #1976d2;
}

.btn-view:hover {
  background: #bbdefb;
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

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-input,
.form-textarea {
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

.form-textarea {
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

.cancel-btn {
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

.server-errors {
  background: #ffe6e6;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.server-errors .error-message {
  margin: 0.25rem 0;
}

@media (max-width: 768px) {
  .admin-container {
    padding: 1rem;
  }

  .header {
    flex-direction: column;
    gap: 1rem;
  }
  .document-name {
  cursor: pointer;
}

.document-name:hover {
  text-decoration: underline;
  color: #1976d2;
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

  .button {
    width: 100%;
    justify-content: center;
  }
}
</style>

<script>
import axios from 'axios'
import Cookies from 'js-cookie'

export default {
  name: 'MyDocuments',
  data() {
    return {
      documents: [],
      loading: true,
      searchQuery: '',
      showAddModal: false,
      newDocument: {
        userId: ''
      },
      selectedFiles: [],
      newDocuments: [],
      isDragging: false,
      uploading: false,
      formSubmitted: false,
      serverErrors: [],
      selectedDocuments: [],
      selectAll: false,
      startDate: '',
      endDate: '',
      showDatePicker: false
    }
  },
  computed: {
    filteredDocuments() {
      let filtered = this.documents

      // Filter by search query
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase()
        filtered = filtered.filter(doc =>
          // Recherche dans le nom du document
          doc.name.toLowerCase().includes(query) ||
          // Recherche dans la description du document
          (doc.description && doc.description.toLowerCase().includes(query)) ||
          // Recherche dans le contenu texte du document
          (doc.text && doc.text.toLowerCase().includes(query)) ||
          // Recherche dans les objets du document
          (doc.allObjects && doc.allObjects.some(obj => 
            obj.toLowerCase().includes(query)
          ))
        )
      }

      // Filter by date range
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

      return filtered
    },
    isFormValid() {
      return (
        this.selectedFiles.length > 0 &&
        this.newDocuments.every(doc => doc.name && doc.name.trim() !== '')
      )
    }
  },
  methods: {
    async fetchMyDocuments() {
      this.loading = true
      try {
        const token = Cookies.get('token')
        const response = await axios.get('api/documents', {
          headers: { Authorization: `Bearer ${token}` }
        })
        this.documents = response.data
      } catch (error) {
        console.error('Erreur lors du chargement des documents:', error)
        this.showAlert('error', 'Erreur lors du chargement des documents')
      } finally {
        this.loading = false
      }
    },
    formatDate(dateString) {
      if (!dateString) return 'N/A'
      const date = new Date(dateString)
      return date.toLocaleDateString('fr-FR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    },
    async downloadDocument(doc) {
  try {
    const token = Cookies.get('token');
    const fileUrl = `api/documents/${doc.id}/file`; // Adaptez selon votre API

    // Option 1: Ouverture dans un nouvel onglet
    window.open(fileUrl, '_blank');

    // Option 2: Téléchargement forcé
    const link = document.createElement('a');
    link.href = fileUrl;
    link.download = `${doc.name}.pdf`;
    link.target = '_blank';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

  } catch (error) {
    console.error('Erreur:', error);
    this.showAlert('error', 'Échec du téléchargement');
  }
},
    async deleteDocument(documentId) {
      if (!confirm('Confirmez la suppression de ce document ?')) return

      try {
        const token = Cookies.get('token')
        await axios.delete(`api/documents/${documentId}`, {
          headers: { Authorization: `Bearer ${token}` }
        })

        this.documents = this.documents.filter(doc => doc.id !== documentId)
        this.showAlert('success', 'Document supprimé avec succès')
      } catch (error) {
        console.error('Erreur lors de la suppression:', error)
        this.showAlert('error', 'Échec de la suppression')
      }
    },
    triggerFileInput() {
      this.$refs.fileInput.click()
    },
    handleDragOver() {
      this.isDragging = true
    },
    handleDragLeave() {
      this.isDragging = false
    },
    handleDrop(e) {
      this.isDragging = false
      const files = Array.from(e.dataTransfer.files)
      this.handleFiles(files)
    },
    handleFileChange(e) {
      const files = Array.from(e.target.files)
      this.handleFiles(files)
    },
    handleFiles(files) {
      const validFiles = files.filter(file => file.type === 'application/pdf' && file.size <= 10 * 1024 * 1024)

      if (validFiles.length !== files.length) {
        alert('Seuls les fichiers PDF (max 10 Mo) sont autorisés.')
      }

      if (validFiles.length > 0) {
        this.selectedFiles = [...this.selectedFiles, ...validFiles]

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

        this.newDocuments = [...this.newDocuments, ...newEntries]
      }
    },
    removeFile(index) {
      this.selectedFiles.splice(index, 1)
      this.newDocuments.splice(index, 1)

      if (this.selectedFiles.length === 0 && this.$refs.fileInput) {
        this.$refs.fileInput.value = ''
      }
    },
    closeAddModal() {
      this.showAddModal = false
      this.newDocument = { userId: '' }
      this.selectedFiles = []
      this.newDocuments = []
      this.isDragging = false
      this.formSubmitted = false
      this.serverErrors = []
    },
    async uploadDocument() {
      this.formSubmitted = true
      this.serverErrors = []

      if (!this.isFormValid) {
        alert('Veuillez remplir tous les champs obligatoires et sélectionner au moins un fichier.')
        return
      }

      this.uploading = true
      let successCount = 0
      let errorCount = 0

      try {
        const token = Cookies.get('token')

        // Upload each file individually
        for (let i = 0; i < this.selectedFiles.length; i++) {
          const file = this.selectedFiles[i]
          const docName = this.newDocuments[i].name
          const docDescription = this.newDocuments[i].description

          const formData = new FormData()
          formData.append('file', file)
          formData.append('name', docName.trim())
          formData.append('description', docDescription.trim())

          try {
            const response = await axios.post('api/documents/add', formData, {
              headers: {
                Authorization: `Bearer ${token}`,
                'Content-Type': 'multipart/form-data'
              }
              
            })

            this.documents.unshift(response.data)
            successCount++
          } catch (error) {
            console.error(`Erreur lors de l'ajout du document ${docName}:`, error)
            errorCount++

            if (error.response?.data?.errors) {
              this.serverErrors.push(`Erreur pour ${docName}: ${Object.values(error.response.data.errors).flat().join(', ')}`)
            } else {
              this.serverErrors.push(`Erreur pour ${docName}: ${error.response?.data?.message || 'Erreur inconnue'}`)
            }
          }
        }

        if (successCount > 0) {
          if (errorCount > 0) {
            this.showAlert('success', `${successCount} document(s) ajouté(s) avec succès. ${errorCount} échec(s).`)
          } else {
            this.showAlert('success', `${successCount} document(s) ajouté(s) avec succès !`)
          }

          this.closeAddModal()
        } else {
          this.showAlert('error', 'Échec de l\'ajout de tous les documents.')
        }
      } catch (error) {
        console.error('Erreur générale lors de l\'upload:', error)
        this.serverErrors.push('Une erreur inattendue s\'est produite.')
        this.showAlert('error', 'Erreur lors de l\'upload des documents')
      } finally {
        this.uploading = false
      }
    },
    viewDocument(document) {
  this.$router.push({ name: 'ConsultDocument', params: { id: document.id } });
},
    showAlert(type, message) {
      const alertDiv = document.createElement('div')
      alertDiv.className = `custom-alert ${type}`
      alertDiv.innerHTML = `
        <i class="bi ${type === 'success' ? 'bi-check-circle' : 'bi-exclamation-circle'}"></i>
        <span>${message}</span>
      `
      document.body.appendChild(alertDiv)

      setTimeout(() => {
        alertDiv.classList.add('show')
        setTimeout(() => {
          alertDiv.classList.remove('show')
          setTimeout(() => document.body.removeChild(alertDiv), 300)
        }, 3000)
      }, 100)
    },

    // Date filter methods
    dateRangeText() {
      if (!this.startDate && !this.endDate) return 'Filtrer par date'
      if (this.startDate === this.endDate && this.startDate)
        return new Date(this.startDate).toLocaleDateString('fr-FR')
      return `${this.startDate ? new Date(this.startDate).toLocaleDateString('fr-FR') : ''} - ${this.endDate ? new Date(this.endDate).toLocaleDateString('fr-FR') : ''}`
    },

    toggleDatePicker() {
      this.showDatePicker = !this.showDatePicker
    },

    clearDateFilter() {
      this.startDate = ''
      this.endDate = ''
      this.showDatePicker = false
    },

    // Méthodes pour la sélection multiple
    toggleSelectAll() {
      this.selectAll = !this.selectAll
      if (this.selectAll) {
        this.selectedDocuments = this.filteredDocuments.map(doc => doc.id)
      } else {
        this.selectedDocuments = []
      }
    },

    toggleSelectDocument(docId) {
      const index = this.selectedDocuments.indexOf(docId)
      if (index === -1) {
        this.selectedDocuments.push(docId)
      } else {
        this.selectedDocuments.splice(index, 1)
      }

      // Mettre à jour l'état de selectAll
      this.selectAll = this.filteredDocuments.length > 0 &&
                      this.selectedDocuments.length === this.filteredDocuments.length
    },

    isSelected(docId) {
      return this.selectedDocuments.includes(docId)
    },

    async deleteSelectedDocuments() {
      if (this.selectedDocuments.length === 0) return

      if (!confirm(`Confirmez la suppression de ${this.selectedDocuments.length} document(s) ?`)) return

      try {
        const token = Cookies.get('token')
        let successCount = 0

        for (const docId of this.selectedDocuments) {
          try {
            await axios.delete(`api/documents/${docId}`, {
              headers: { Authorization: `Bearer ${token}` }
            })
            successCount++
          } catch (error) {
            console.error(`Erreur lors de la suppression du document ${docId}:`, error)
          }
        }

        // Mettre à jour la liste des documents
        this.documents = this.documents.filter(doc => !this.selectedDocuments.includes(doc.id))
        this.selectedDocuments = []
        this.selectAll = false

        this.showAlert('success', `${successCount} document(s) supprimé(s) avec succès`)
      } catch (error) {
        console.error('Erreur lors de la suppression des documents:', error)
        this.showAlert('error', 'Échec de la suppression des documents')
      }
    },

    downloadSelectedDocuments() {
      if (this.selectedDocuments.length === 0) return

      // Télécharger chaque document sélectionné
      for (const docId of this.selectedDocuments) {
        const doc = this.documents.find(d => d.id === docId)
        if (doc) {
          this.downloadDocument(doc)
        }
      }
    }
  },
  mounted() {
    this.fetchMyDocuments()
  }
}
</script>



