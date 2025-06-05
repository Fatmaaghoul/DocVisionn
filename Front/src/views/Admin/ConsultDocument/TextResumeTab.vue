<template>
  <div class="text-tab">
    <div class="text-section">
      <h3>Texte extrait</h3>
      <div class="textarea-container">
        <textarea readonly :value="document.text"></textarea>
        <button @click="copyText" class="copy-btn" title="Copier le texte">
          <i :class="showTextCopied ? 'bi bi-check' : 'bi bi-clipboard'"></i>
        </button>
      </div>
      <div v-if="showTextCopied" class="notification">Texte copié !</div>
    </div>

    <div class="resume-section">
      <div class="resume-header">
        <h3>Résumé du document</h3>
        <button 
          @click="generateResumer" 
          :disabled="documentStore.isAnalysing"
          class="generate-btn"
        >
          <template v-if="documentStore.isAnalysing">
            <i class="bi bi-arrow-clockwise spin-icon"></i> Génération...
          </template>
          <template v-else>
            <i class="bi bi-arrow-clockwise"></i>
            {{ document.resumer ? 'Régénérer' : 'Générer' }}
          </template>
        </button>
      </div>
      <div class="resume-content">
        <div v-if="documentStore.isAnalysing" class="loading-container">
          <div class="loading-spinner"></div>
          <p>Génération du résumé en cours...</p>
        </div>
        <template v-else>
          <div v-if="document.resumer" class="resume-text-container">
            <p class="resume-text">{{ document.resumer }}</p>
            <button @click="copyResume" class="copy-btn" title="Copier le résumé">
              <i :class="showResumeCopied ? 'bi bi-check' : 'bi bi-clipboard'"></i>
            </button>
            <div v-if="showResumeCopied" class="notification">Résumé copié !</div>
          </div>
          <p v-else class="no-resume">Aucun résumé disponible</p>
        </template>
      </div>
    </div>
  </div>
</template>

<script>
import { useDocumentStore } from '@/Store/analysis.js';
import { ref, computed, watch } from 'vue';

export default {
  props: {
    document: {
      type: Object,
      required: true
    }
  },
  emits: ['generate-resumer', 'copy-text'],
  setup(props, { emit }) {
    const documentStore = useDocumentStore();
    
    // Fonction locale pour générer le résumé
    const generateResumer = () => {
      console.log('Demande de génération de résumé, état actuel:', documentStore.isAnalysing);
      emit('generate-resumer');
    };
    
    // Observer les changements d'état
    watch(() => documentStore.isAnalysing, (newVal) => {
      console.log('État d\'analyse changé dans TextResumeTab:', newVal);
    });
    
    return {
      documentStore,
      generateResumer
    };
  },
  data() {
    return {
      showTextCopied: false,
      showResumeCopied: false
    };
  },
  methods: {
    copyText() {
      navigator.clipboard.writeText(this.document.text);
      this.showTextCopied = true;
      setTimeout(() => {
        this.showTextCopied = false;
      }, 2000);
      this.$emit('copy-text');
    },
    copyResume() {
      navigator.clipboard.writeText(this.document.resumer);
      this.showResumeCopied = true;
      setTimeout(() => {
        this.showResumeCopied = false;
      }, 2000);
    }
  }
};
</script>

<style scoped>
.text-tab {
  font-family: 'Segoe UI', Roboto, 'Helvetica Neue', sans-serif;
  max-width: 900px;
  margin: 0 auto;
  padding: 20px;
}

.text-section {
  position: relative;
}

.textarea-container {
  position: relative;
}

.text-section textarea {
  width: 100%;
  min-height: 300px;
  padding: 15px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  resize: vertical;
  font-size: 14px;
  line-height: 1.5;
  background: #fafafa;
  transition: border 0.3s ease;
}

.text-section textarea:focus {
  outline: none;
  border-color: #2196F3;
}

.copy-btn {
  position: absolute;
  top: 10px;
  right: 10px;
  background-color: #4F46E5;
  color: white;
  border: none;
  border-radius: 50%;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 1rem;
  transition: all 0.3s ease;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
  z-index: 2;
}

.copy-btn:hover {
  background-color: #4338CA;
  transform: scale(1.1);
}

.copy-btn:active {
  transform: scale(0.95);
}

.copy-btn i {
  transition: all 0.3s ease;
}

.resume-section {
  margin-top: 30px;
}

.resume-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.generate-btn {
  background: #4F46E5;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: all 0.3s ease;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}

.generate-btn:hover:not(:disabled) {
  background: #4338CA;
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
}

.generate-btn:disabled {
  background: #9CA3AF;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.resume-content {
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e9ecef;
  min-height: 150px;
  position: relative;
}

.resume-text-container {
  position: relative;
  padding-right: 40px;
}

.no-resume {
  color: #666;
  font-style: italic;
  margin: 0;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 20px 0;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(79, 70, 229, 0.2);
  border-radius: 50%;
  border-top-color: #4F46E5;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.spin-icon {
  animation: spin 1s linear infinite;
}

.resume-text {
  margin-bottom: 15px;
  line-height: 1.6;
  white-space: pre-wrap;
}

.notification {
  position: fixed;
  top: 20px;
  right: 20px;
  background-color: #4F46E5;
  color: white;
  padding: 10px 20px;
  border-radius: 8px;
  font-size: 0.9rem;
  z-index: 1000;
  display: flex;
  align-items: center;
  gap: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  animation: fadeIn 0.3s ease-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
