<template>
  <div class="consult-container">
    <!-- Header avec bouton retour -->
    <div class="header">
      <button @click="$router.go(-1)" class="back-button">
        <i class="bi bi-arrow-left"></i> Retour
      </button>
      <h1>{{ document.name }}</h1>
      <div class="document-status">
        <span :class="['status-badge', document.isExtracted ? 'extracted' : 'pending']">
          {{ document.isExtracted ? 'Extrait' : 'Non extrait' }}
        </span>
        <span :class="['status-badge', document.isAnalysed ? 'analysed' : 'not-analysed']">
          {{ document.isAnalysed ? 'Analysé' : 'Non analysé' }}
        </span>
      </div>
    </div>

    <!-- Barre d'onglets -->
    <div class="tabs">
      <button 
        v-for="tab in tabs" 
        :key="tab.id"
        @click="activeTab = tab.id"
        :class="{ 'active': activeTab === tab.id }"
      >
        <i :class="tab.icon"></i> {{ tab.label }}
      </button>
    </div>

    <!-- <div v-if="loadingStates.analysis" class="loading-overlay">
      <div class="spinner"></div>
      <p>Analyse en cours...</p>
    </div>
    <div v-if="loadingStates.extraction" class="loading-overlay">
      <div class="spinner-large"></div>
      <p>Extraction en cours... Cette opération peut prendre plusieurs minutes</p>
    </div> -->

    <!-- Contenu des onglets -->
    <div class="tab-content">
      <ConsultationTab 
        v-if="activeTab === 'consult'" 
        :document="document"
      />
      
      <EditionTab 
        v-if="activeTab === 'edit'"
        :document="document"
        @update-document="updateDocument"
      />
      
      <AnalyseImageTab 
        v-if="activeTab === 'analyse'"
        :document="document"
        :images="images"
        :loading-states="loadingStates"
        @analyse-document="analyseDocument"
        @describe-objects="describeObjects"
      />
      
      <TextResumeTab 
        v-if="activeTab === 'text'"
        :document="document"
        @generate-resumer="generateResumer"
        @copy-text="copyText"
      />
    </div>
  </div>
</template>

<script>
import ConsultationTab from './ConsultDocument/ConsultationTab.vue';
import EditionTab from './ConsultDocument/EditionTab.vue';
import AnalyseImageTab from './ConsultDocument/AnalyseImageTab.vue';
import TextResumeTab from './ConsultDocument/TextResumeTab.vue';
import axios from 'axios';
import Cookies from 'js-cookie';
import { useDocumentStore } from '@/Store/analysis.js';

export default {
  components: {
    ConsultationTab,
    EditionTab,
    AnalyseImageTab,
    TextResumeTab
  },
  setup() {
    const documentStore = useDocumentStore();
    return { documentStore };
  },
  data() {
    return {
      document: {
        id: null,
        name: '',
        description: '',
        fileUrl: '',
        uploadDate: null,
        isExtracted: false,
        isAnalysed: false,
        text: null,
        resumer: null,
        ownerId: null
      },
      images: [],
      loadingStates: {
        extraction: false,
        document: false,
        analysis: false
      },
      activeTab: 'consult',
      tabs: [
        { id: 'consult', label: 'Consultation', icon: 'bi bi-eye' },
        { id: 'edit', label: 'Édition', icon: 'bi bi-pencil' },
        { id: 'analyse', label: 'Analyse Images', icon: 'bi bi-image' },
        { id: 'text', label: 'Texte & Résumé', icon: 'bi bi-file-text' }
      ]
    };
  },
  async created() {
    await this.fetchDocument();
  },
  methods: {
    async fetchDocument() {
      try {
        this.loadingStates.document = true;
        const token = Cookies.get('token');
        const response = await axios.get(`/api/documents/${this.$route.params.id}`, {
          headers: { Authorization: `Bearer ${token}` }
        });
        
        this.document = response.data;
        
        if (!this.document.isExtracted) {
          await this.extractContent();
        } else {
          await this.fetchImages();
        }
        
      } catch (error) {
        console.error('Error fetching document:', error);
      } finally {
        this.loadingStates.document = false;
      }
    },
    
    async extractContent() {
      try {
        this.loadingStates.extraction = true;
        // Utiliser le store pour gérer l'état d'extraction
        this.documentStore.startExtracting(this.document.name);
        localStorage.setItem('isExtracting', 'true');
        
        const token = Cookies.get('token');
        
        await axios.post(
          `/api/documents/extract/${this.$route.params.id}`,
          {},
          { headers: { Authorization: `Bearer ${token}` } }
        );

        this.document.isExtracted = true;
        await this.fetchImages();

      } catch (error) {
        console.error("Erreur lors de l'extraction:", error);
      } finally {
        this.loadingStates.extraction = false;
        this.documentStore.stopExtracting();
        localStorage.setItem('isExtracting', 'false');
      }
    },
    
    async fetchImages() {
      try {
        const token = Cookies.get('token');
        const response = await axios.get(`/api/images/${this.$route.params.id}`, {
          headers: { Authorization: `Bearer ${token}` }
        });

        this.images = response.data.map(image => ({
          ...image,
          objects: []
        }));

      } catch (error) {
        console.error('Error fetching images:', error);
      }
    },
    
    async analyseDocument() {
      try {
        this.loadingStates.analysis = true;
        this.documentStore.startAnalysing('analyse', this.document.name);
        const token = Cookies.get('token');

        await axios.post(
          `/api/documents/analyze/${this.$route.params.id}`,
          {},
          { headers: { Authorization: `Bearer ${token}` } }
        );

        this.document.isAnalysed = true;
        await this.fetchImages();

      } catch (error) {
        console.error("Erreur analyse:", error);
      } finally {
        this.loadingStates.analysis = false;
        this.documentStore.stopAnalysing();
      }
    },
    
    async describeObjects(imageId, objectIds) {
      // Implémentation de la méthode
      this.documentStore.startAnalysing('description', this.getImageName(imageId));
      try {
        // Logique de description des objets
      } catch (error) {
        console.error("Erreur description:", error);
      } finally {
        this.documentStore.stopAnalysing();
      }
    },
    
    async updateDocument(updatedData) {
      try {
        const token = Cookies.get('token');
        await axios.put(`/api/documents/${this.$route.params.id}`, updatedData, {
          headers: { Authorization: `Bearer ${token}` }
        });
        
        this.document = { ...this.document, ...updatedData };
        
      } catch (error) {
        console.error('Erreur lors de la mise à jour:', error);
      }
    },
    
    async generateResumer() {
      try {
        this.documentStore.startAnalysing('resume', this.document.name);
        const token = Cookies.get('token');
        const response = await axios.post(
          `/api/documents/summarize/${this.$route.params.id}`,
          {},
          { headers: { Authorization: `Bearer ${token}` } }
        );
        
        this.document.resumer = response.data.summary || response.data.resumer;
        
      } catch (error) {
        console.error('Erreur lors de la génération du résumé:', error);
      } finally {
        this.documentStore.stopAnalysing();
      }
    },
    
    async copyText() {
      try {
        await navigator.clipboard.writeText(this.document.text);
      } catch (error) {
        console.error('Error copying text:', error);
      }
    }
  }
};
</script>

<style scoped>
/* Styles du conteneur principal */
.consult-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.header {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 30px;
  flex-wrap: wrap;
}

.back-button {
    background: var(#4361ee);

  background: #f0f0f0;
  border: none;
  padding: 8px 15px;
  border-radius: 20px;
  cursor: pointer;
  display: flex;
  align-items: center;
    color: var(#1b263b);

  gap: 5px;
  color: #405569;
}
.back-button:hover {
  background: white;
  transform: translateY(-2px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
  color: var(#4361ee);
}

.back-button:hover::before {
  left: 100%;
}

.back-button i {
  transition: transform 0.3s ease;
}

.back-button:hover i {
  transform: translateX(-3px);
}

.document-status {
  display: flex;
  gap: 10px;
  margin-left: auto;
}

.status-badge {
  padding: 5px 10px;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 500;
}

.status-badge.extracted {
  background: #e8f5e9;
  color: #2e7d32;
}

.status-badge.pending {
  background: #fff8e1;
  color: #ff8f00;
}

.status-badge.analysed {
  background: #e3f2fd;
  color: #1565c0;
}

.status-badge.not-analysed {
  background: #ffebee;
  color: #c62828;
}

.tabs {
  display: flex;
  border-bottom: 1px solid #ddd;
  margin-bottom: 20px;
}

.tabs button {
  padding: 10px 20px;
  background: none;
  border: none;
  border-bottom: 3px solid transparent;
  cursor: pointer;
  font-size: 1rem;
  display: flex;
  align-items: center;
  gap: 8px;
  color: #405569;
}

.tabs button:hover {
  background: #f5f5f5;
}

.tabs button.active {
  border-bottom-color: #1976d2;
  color: #1976d2;
  font-weight: 500;
}

.tab-content {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.8);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.spinner {
  display: inline-block;
  width: 50px;
  height: 50px;
  border: 5px solid rgba(0, 0, 0, 0.1);
  border-radius: 50%;
  border-top-color: #1976d2;
  animation: spin 1s ease-in-out infinite;
}

.spinner-large {
  width: 80px;
  height: 80px;
  border: 8px solid #f3f3f3;
  border-top: 8px solid #3498db;
  border-radius: 50%;
  animation: spin 1.5s linear infinite;
  margin-bottom: 20px;
}

/* Animations */
@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Responsive */
@media (max-width: 768px) {
  .header {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .document-status {
    margin-left: 0;
    margin-top: 10px;
    width: 100%;
  }
  
  .tabs {
    overflow-x: auto;
    padding-bottom: 5px;
  }
  
  .tabs button {
    padding: 8px 12px;
    font-size: 0.9rem;
  }
}
</style>




