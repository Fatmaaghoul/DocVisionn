<template>
  <div class="resumer-container">
    <!-- Contenu du résumé -->
    <div class="resumer-content">
      <!-- Bouton Copier en haut à droite -->
      <button
        v-if="resumer && resumer !== ''"
        class="copy-btn"
        @click="copyResumer"
        title="Copier le résumé"
      >
        <i :class="isCopied ? 'bi bi-check' : 'bi bi-clipboard'"></i>
      </button>

      <p v-if="!resumer || resumer==''">Aucun résumé</p>
      <p v-else>{{ resumer }}</p>

      <!-- Bouton Régénérer modifié -->
      <button
        class="regenerate-btn"
        @click="generateResumer"
        :disabled="isGenerating || documentStore.isAnalysing"
      >
        <i class="bi bi-arrow-clockwise" :class="{ 'spin-icon': isGenerating }"></i>
        {{ isGenerating ? 'En cours...' : resumer ? 'Régénérer le résumé' : 'Générer un résumé' }}
      </button>
    </div>

    <!-- Notification de copie -->
    <div v-if="isCopied" class="copy-notification">
      Résumé copié ! ✅
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'
import { useDocumentStore } from '@/Store/analysis.js'

const props = defineProps({
  resumer: {
    type: String,
    default: ''
  },
  documentId: {
    type: String,
    required: true
  },
  document: {
    type: Object,
    default: () => ({})
  }
})

const emit = defineEmits(['update-resumer'])
const documentStore = useDocumentStore()

const isGenerating = ref(false)
const isCopied = ref(false)

const generateResumer = async () => {
  isGenerating.value = true
  documentStore.startAnalysing('resume', props.document.name) // Ajouter le nom du document
  try {
    const response = await axios.post(`/api/documents/summarize/${props.documentId}`)
    const newResumer = response.data.summary
    emit('update-resumer', newResumer)
    alert('Résumé généré avec succès ✅')
  } catch (error) {
    console.error('Erreur lors de la génération du résumé :', error)
    alert(`❌ Échec de la génération du résumé : ${error.response?.data?.message || error.message}`)
  } finally {
    isGenerating.value = false
    documentStore.stopAnalysing()
  }
}

const copyResumer = () => {
  if (props.resumer && props.resumer !== '') {
    navigator.clipboard.writeText(props.resumer).then(() => {
      isCopied.value = true
      setTimeout(() => {
        isCopied.value = false
      }, 2000) // L'icône redevient bi-clipboard après 2 secondes
    })
  }
}
</script>

<style scoped>
.resumer-container {
  padding: 20px;
  background-color: #f8f9fa;
  border-radius: 8px;
  min-height: 200px;
}

.resumer-content {
  position: relative; /* Pour positionner le bouton Copier */
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.resumer-content h3 {
  font-size: 1.5rem;
  color: #333;
  margin: 0;
}

.resumer-content p {
  font-size: 1rem;
  color: #555;
  line-height: 1.6;
  margin: 0;
}

.no-resumer-text {
  font-size: 1.1rem;
  color: #666;
  margin: 0;
}

.copy-btn {
  position: absolute;
  top: 0;
  right: 0;
  background-color: #28a745;
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
  transition: background-color 0.3s, transform 0.2s;
}

.copy-btn:hover {
  background-color: #218838;
  transform: scale(1.1);
}

.copy-btn i {
  transition: all 0.3s ease; /* Transition douce pour le changement d'icône */
}

.regenerate-btn {
  background-color: #007bff; /* Changement de couleur pour une meilleure distinction */
  color: white;
  padding: 12px 24px; /* Légèrement plus grand */
  border: none;
  border-radius: 12px; /* Coins plus arrondis */
  cursor: pointer;
  font-size: 1.1rem; /* Texte légèrement plus grand */
  display: flex;
  align-items: center;
  gap: 10px;
  transition: background-color 0.3s, transform 0.2s; /* Ajout d'une transition pour l'échelle */
  align-self: flex-end; /* Déplacé à droite */
}

.regenerate-btn:hover {
  background-color: #0056b3; /* Couleur plus sombre au survol */
  transform: scale(1.05); /* Légère mise à l'échelle au survol */
}

.regenerate-btn:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
  transform: none; /* Pas de scale quand désactivé */
}

.copy-notification {
  position: fixed;
  top: 20px;
  right: 20px;
  background-color: #28a745;
  color: white;
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 0.8rem;
  z-index: 1000;
  display: flex;
  align-items: center;
  gap: 8px;
}

.spin-icon {
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>



