<template>
  <div class="modal-overlay" @click="$emit('close')">
    <div class="modal-content" @click.stop>
      <div class="modal-header">
        <h2>Ajouter des documents</h2>
        <button class="close-btn" @click="$emit('close')">
          <i class="bi bi-x"></i>
        </button>
      </div>
      <div class="modal-body">
        <div class="form-group">
          <label>Fichiers PDF</label>
          <div
            class="file-upload"
            @dragover.prevent="handleDragOver"
            @dragleave.prevent="handleDragLeave"
            @drop.prevent="handleDrop"
            :class="{ 'dragging': isDragging }"
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
            <div class="upload-placeholder" v-if="!selectedFiles.length">
              <i class="bi bi-cloud-upload"></i>
              <p>Faites glisser vos fichiers PDF ici ou cliquez pour parcourir</p>
              <button class="browse-btn">Parcourir les fichiers</button>
            </div>
            <div class="file-preview-container" v-else>
              <div class="file-preview" v-for="(file, index) in selectedFiles" :key="index">
                <span class="file-name">{{ file.name }}</span>
                <button class="remove-file" @click.stop="removeFile(index)">×</button>
              </div>
            </div>
          </div>
          <span v-if="!selectedFiles.length && formSubmitted" class="error-message">
            Veuillez sélectionner au moins un fichier PDF.
          </span>
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
                :placeholder="'Description pour ' + file.name"
              ></textarea>
            </div>
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <button class="cancel-btn" @click="$emit('close')">Annuler</button>
        <button
          class="submit-btn"
          @click="uploadDocuments"
          :disabled="!isFormValid || uploading"
        >
          <span v-if="uploading" class="spinner"></span>
          <span v-else>Ajouter</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'AddDocumentModal',
  data() {
    return {
      isDragging: false,
      selectedFiles: [],
      uploading: false,
      formSubmitted: false,
      newDocuments: []
    }
  },
  computed: {
    isFormValid() {
      return (
        this.selectedFiles.length > 0 &&
        this.newDocuments.every(doc => doc.name.trim() !== '')
      )
    }
  },
  methods: {
    handleDragOver() {
      this.isDragging = true
    },
    handleDragLeave() {
      this.isDragging = false
    },
    handleDrop(event) {
      this.isDragging = false
      const files = Array.from(event.dataTransfer.files)
      this.handleFiles(files)
    },
    handleFileChange(event) {
      const files = Array.from(event.target.files)
      this.handleFiles(files)
    },
    handleFiles(files) {
      const validFiles = files.filter(file => file.type === 'application/pdf')
      if (validFiles.length !== files.length) {
        alert('Seuls les fichiers PDF sont autorisés.')
      }
      this.selectedFiles = [...this.selectedFiles, ...validFiles]
      this.newDocuments = [
        ...this.newDocuments,
        ...validFiles.map(file => {
          const fileName = file.name.replace(/\.[^/.]+$/, '') // Remove file extension
          
          // Create a default description like in MyDocuments.vue
          const defaultDescription = `Pas de description .`
          
          return {
            name: fileName,
            description: defaultDescription
          }
        })
      ]
    },
    removeFile(index) {
      this.selectedFiles.splice(index, 1)
      this.newDocuments.splice(index, 1)
      if (this.$refs.fileInput) {
        this.$refs.fileInput.value = ''
      }
    },
    triggerFileInput() {
      this.$refs.fileInput.click()
    },
    uploadDocuments() {
      this.formSubmitted = true
      if (!this.isFormValid) {
        alert('Veuillez remplir tous les champs obligatoires.')
        return
      }
      this.uploading = true
      const documentsToUpload = this.selectedFiles.map((file, index) => ({
        file,
        name: this.newDocuments[index].name,
        description: this.newDocuments[index].description || ''
      }))
      console.log('Documents à envoyer :', JSON.stringify(documentsToUpload.map(doc => ({
        name: doc.name,
        description: doc.description,
        fileName: doc.file.name
      })), null, 2))
      this.$emit('upload-documents', documentsToUpload)
      this.uploading = false
      this.resetForm()
    },
    resetForm() {
      this.newDocuments = []
      this.selectedFiles = []
      this.formSubmitted = false
      if (this.$refs.fileInput) {
        this.$refs.fileInput.value = ''
      }
      this.isDragging = false
    }
  },
  emits: ['close', 'upload-documents']
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(2px);
}

.modal-content {
  background: linear-gradient(145deg, #ffffff, #f8fafc);
  border-radius: 16px;
  width: 90%;
  max-width: 640px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.15);
  animation: modalFadeIn 0.4s ease;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

@keyframes modalFadeIn {
  from {
    opacity: 0;
    transform: translateY(-30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.5rem 2rem;
  border-bottom: 1px solid #e2e8f0;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.75rem;
  font-weight: 600;
  color: #1e293b;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.75rem;
  color: #64748b;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 50%;
  transition: all 0.3s ease;
}

.close-btn:hover {
  background: #f1f5f9;
  color: #ef4444;
}

.modal-body {
  padding: 2rem;
}

.form-group {
  margin-bottom: 1.75rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.75rem;
  font-weight: 500;
  font-size: 1rem;
  color: #1e293b;
}

.form-input {
  width: 100%;
  padding: 0.875rem;
  border: 1px solid #d1d5db;
  border-radius: 10px;
  font-size: 1rem;
  background: #ffffff;
  transition: all 0.2s ease;
}

.form-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-input.error {
  border-color: #ef4444;
}

.form-textarea {
  width: 100%;
  padding: 0.875rem;
  border: 1px solid #d1d5db;
  border-radius: 10px;
  font-size: 1rem;
  background: #ffffff;
  transition: all 0.2s ease;
  resize: vertical;
  min-height: 100px;
}

.form-textarea:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.error-message {
  color: #ef4444;
  font-size: 0.875rem;
  margin-top: 0.5rem;
  display: block;
}

.file-upload {
  border: 2px dashed #d1d5db;
  border-radius: 12px;
  padding: 2rem;
  text-align: center;
  cursor: pointer;
  background: #f8fafc;
  transition: all 0.3s ease;
}

.file-upload.dragging {
  border-color: #3b82f6;
  background: #eff6ff;
}

.upload-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  color: #64748b;
}

.upload-placeholder i {
  font-size: 2.5rem;
  color: #3b82f6;
  transition: transform 0.3s ease;
}

.file-upload:hover .upload-placeholder i {
  transform: scale(1.1);
}

.upload-placeholder p {
  margin: 0;
  font-size: 1rem;
}

.browse-btn {
  margin-top: 1rem;
  padding: 0.5rem 1.5rem;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 0.95rem;
  font-weight: 500;
  transition: all 0.2s ease;
  box-shadow: 0 2px 8px rgba(59, 130, 246, 0.2);
}

.browse-btn:hover {
  background: #2563eb;
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
}

.file-input {
  display: none;
}

.file-preview-container {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.file-preview {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #f1f5f9;
  padding: 0.75rem 1rem;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
}

.file-name {
  font-size: 0.95rem;
  color: #1e293b;
  font-weight: 500;
}

.remove-file {
  background: none;
  border: none;
  color: #ef4444;
  cursor: pointer;
  font-size: 1.5rem;
  transition: all 0.2s ease;
}

.remove-file:hover {
  color: #dc2626;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding: 1.5rem 2rem;
  border-top: 1px solid #e2e8f0;
}

.submit-btn,
.cancel-btn {
  padding: 0.75rem 2rem;
  border-radius: 10px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.2s ease;
}

.submit-btn {
  background: #3b82f6;
  color: white;
  border: none;
  box-shadow: 0 2px 8px rgba(59, 130, 246, 0.2);
}

.submit-btn:hover:not(:disabled) {
  background: #2563eb;
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
}

.submit-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.cancel-btn {
  background: #ffffff;
  border: 1px solid #d1d5db;
  color: #64748b;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.cancel-btn:hover {
  background: #f1f5f9;
  border-color: #9ca3af;
}

.spinner {
  width: 1.5rem;
  height: 1.5rem;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.form-fields {
  margin-bottom: 1.5rem;
  max-height: 400px;
  overflow-y: auto;
  padding-right: 10px;
  animation: fadeIn 0.4s ease;
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

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-15px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 768px) {
  .modal-content {
    width: 95%;
    border-radius: 12px;
  }

  .modal-header {
    padding: 1rem 1.5rem;
  }

  .modal-body {
    padding: 1.5rem;
  }

  .modal-footer {
    flex-direction: column;
    padding: 1rem 1.5rem;
  }

  .submit-btn,
  .cancel-btn {
    width: 100%;
    justify-content: center;
    padding: 0.75rem;
  }
}
</style>





