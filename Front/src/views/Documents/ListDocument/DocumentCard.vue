<template>
  <div class="document-card">
    <div :class="['status-bar', document.isAnalysed ? 'traiter' : 'non-traiter']"></div>

    <div class="document-content">
      <h3 class="document-title">{{ document.name }}</h3>
      
      <div class="page-preview-container">
        <canvas ref="pdfCanvas" class="pdf-canvas"></canvas>
        <div v-if="loading" class="loading-overlay">
          <div class="spinner"></div>
        </div>
        <div v-if="error" class="error-message">
          Impossible de charger la prévisualisation
        </div>
      </div>
      <div class="document-meta">
        <p class="document-description" v-if="document.description">{{ document.description }}</p>
        <p class="document-date">{{ formatDate(document.uploadDate) }}</p>
      </div>
    </div>

    <div class="document-actions">
      <button class="action-btn view-btn" @click="$emit('navigate-to-content', document)">
        <i class="bi bi-eye"></i>
      </button>
      <button class="action-btn download-btn" @click="downloadDocument">
        <i class="bi bi-download"></i>
      </button>
      <button class="action-btn delete-btn" @click="$emit('delete-document', document.id)">
        <i class="bi bi-trash"></i>
      </button>
    </div>
  </div>
</template>

<script>
import * as pdfjsLib from 'pdfjs-dist';
import workerUrl from 'pdfjs-dist/build/pdf.worker.min.js?url';
import axios from 'axios';
import Cookies from 'js-cookie';

export default {
  name: 'DocumentCard',
  props: {
    document: {
      type: Object,
       required: true
    }
  },
  data() {
    return {
      loading: false,
      error: false
    }
  },
  created() {
    pdfjsLib.GlobalWorkerOptions.workerSrc = workerUrl;
  },
  async mounted() {
    if (this.document.fileUrl && this.document.fileUrl.endsWith('.pdf')) {
      await this.renderPdfPreview();
    }
  },
  methods: {
    formatDate(dateString) {
      if (!dateString) return '';
      const date = new Date(dateString);
      return date.toLocaleDateString('fr-FR', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      });
    },
    async renderPdfPreview() {
      try {
        this.loading = true;
        this.error = false;
        
        const pdf = await pdfjsLib.getDocument({
          url: this.document.fileUrl,
          disableWorker: false
        }).promise;

        const page = await pdf.getPage(1);
        const viewport = page.getViewport({ scale: 0.8 });
        
        const canvas = this.$refs.pdfCanvas;
        const context = canvas.getContext('2d');
        
        canvas.height = viewport.height;
        canvas.width = viewport.width;
        
        await page.render({
          canvasContext: context,
          viewport: viewport
        }).promise;
        
      } catch (error) {
        console.error("Erreur de rendu PDF:", error);
        this.error = true;
      } finally {
        this.loading = false;
      }
    },
    async downloadDocument() {
      try {
        const token = Cookies.get('token');
        const response = await axios({
          url: this.document.fileUrl,
          method: 'GET',
          responseType: 'blob',
          headers: {
            Authorization: `Bearer ${token}`
          }
        });

        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `${this.document.name}.pdf`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
      } catch (error) {
        console.error('Erreur lors du téléchargement:', error);
        alert('Erreur lors du téléchargement du document. Veuillez réessayer.');
      }
    }
  }
}
</script>

<style scoped>
.document-card {
  position: relative;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
  height: 100%; 
}

.document-content {
  padding: 1rem;
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
  gap: 0.5rem;
}

.document-title {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
  color: #2c3e50;
  word-break: break-word;
}

.document-meta {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
  margin-bottom: 0.1rem;
}

.document-description {
  margin: 0;
  font-size: 0.85rem;
  color: #6c757d;
  display: -webkit-box;
  
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
}

.document-date {
  font-size: 0.8rem;
  color: #adb5bd;
  margin: 0;
}

.page-preview-container {
  flex: 1;
  position: relative;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: #f5f5f5;
  border: 1px solid #eee;
  border-radius: 4px;
  overflow: hidden;
  min-height: 0;
  max-height: 100px;
}

.pdf-canvas {
  width: 100%;
  height: auto;
  object-fit: contain;
  transform: none;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: rgba(255, 255, 255, 0.7);
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(0, 0, 0, 0.1);
  border-radius: 50%;
  border-top-color: #0d6efd;
  animation: spin 1s ease-in-out infinite;
}

.error-message {
  color: #dc3545;
  padding: 1rem;
  text-align: center;
}

.document-actions {
  display: flex;
  border-top: 1px solid #f0f0f0;
  padding: 0.75rem;
  gap: 0.5rem;
  flex-shrink: 0;
}

.action-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0.5rem;
  border: none;
  border-radius: 6px;
  background: none;
  color: #6c757d;
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-btn:hover {
  background-color: #f8f9fa;
}

.view-btn:hover {
  color: #0d6efd;
}

.download-btn:hover {
  color: #198754;
}

.delete-btn:hover {
  color: #dc3545;
}

.status-bar {
  position: absolute;
  top: 0;
  right: 0;
  width: 6px;
  height: 100%;
  z-index: 1;
}

.status-bar.traiter {
  background-color: #28a745;
}

.status-bar.non-traiter {
  background-color: #dc3545;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>


