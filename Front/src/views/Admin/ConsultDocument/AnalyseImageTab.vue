<template>
  <div class="analyse-tab">
    <!-- Bouton d'analyse globale -->
    <div class="global-analysis-controls">
      <button v-if="!document.isAnalysed"
        @click="$emit('analyse-document')"
        :disabled="isAnalyzing || !document.isExtracted "
        class="analyse-document-btn"
      >
        <i class="bi bi-gear-wide-connected" :class="{ 'spin-icon': isAnalyzing }"></i>
        {{ isAnalyzing ? 'Analyse en cours...' : 'Analyser ' }}
      </button>
      <p v-if="!document.isExtracted" class="analysis-warning">
        <i class="bi bi-exclamation-triangle"></i> Le document doit d'abord être extrait
      </p>
    </div>

    <!-- Section des images -->
    <div class="images-section">
      <div class="images-list" v-if="enrichedImages.length > 0">
        <div class="image-card" v-for="img in enrichedImages" :key="img.id">
          <div class="image-content-wrapper">
            <!-- Colonne de gauche - Image -->
            <div class="image-column">
              <div class="image-container">
                <img :src="img.fileUrl" alt="Image du document" />
                <div class="image-actions">
                  <button class="zoom-btn" @click="showFullImage(img.fileUrl)" title="Agrandir">
                    <i class="bi bi-zoom-in"></i>
                  </button>
                  <button class="download-img-btn" @click="downloadImage(img.fileUrl)" title="Télécharger l'image">
                    <i class="bi bi-download"></i>
                  </button>
                </div>
              </div>
            </div>
            
            <!-- Colonne de droite - Tableau -->
            <div class="table-column">
              <div class="objects-list modern-table-container">
                <div v-if="img.objects && img.objects.length > 0">
                  <h3 class="table-title">Analyse des objets</h3>
                  <div class="table-responsive">
                    <table class="modern-table">
                      <!-- En-tête du tableau -->
                      <thead>
                        <tr>
                          <th>Sélection</th>
                          <th>Objet</th>
                          <th>Texte</th>
                          <th>Image</th>
                          <th>%</th>
                        </tr>
                      </thead>
                      <!-- Corps du tableau -->
                      <tbody>
                        <tr v-for="obj in img.objects" :key="obj.id">
                          <td>
                            <label class="custom-checkbox">
                              <input
                                type="checkbox"
                                :value="obj.id"
                                v-model="selectedObjects[img.id]"
                                @change="updateSelectedObjects(img.id)"
                              />
                              <span class="checkmark"></span>
                            </label>
                          </td>
                          <td>{{ obj.name || '-' }}</td>
                          <td>{{ obj.occurenceText }}</td>
                          <td>{{ obj.occurenceImage }}</td>
                          <td>
                            <span class="percentage-badge" :class="getPercentageClass(obj.pourcentage)">
                              {{ obj.pourcentage ? obj.pourcentage.toFixed(2) + '%' : '-' }}
                            </span>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
                <p v-else class="no-objects-message">Aucun objet détecté</p>
              </div>
              
              <!-- Boutons d'action -->
              <div class="action-buttons">
                <button
                  class="describe-btn modern-action-btn primary"
                  :disabled="!hasSelectedObjects(img.id) || documentStore.isAnalysing"
                  @click="describeObjects(img.id)"
                >
                  <i class="bi bi-chat-square-text"></i>
                  <span>Générer description</span>
                  <span v-if="documentStore.isAnalysing && timers[img.id] > 0" class="spinner"></span>
                </button>
                
                <button
                  v-if="img.descriptions && img.descriptions.length > 0"
                  class="toggle-descriptions-btn modern-action-btn secondary"
                  @click="toggleDescriptions(img.id)"
                >
                  <i :class="showDescriptions[img.id] ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                  <span>{{ showDescriptions[img.id] ? 'Masquer' : 'Afficher' }} descriptions</span>
                </button>
              </div>
            </div>
          </div>
          
          <!-- Section des descriptions -->
          <div
            class="descriptions-section modern-descriptions"
            v-if="showDescriptions[img.id] && img.descriptions && img.descriptions.length > 0"
          >
            <div class="descriptions-header">
              <h3>Descriptions</h3>
            </div>
            
            <div v-if="displayedDescriptions(img.descriptions, img.id).length === 0">
              <p class="no-descriptions">Aucune description correspondant à la sélection actuelle</p>
            </div>
            
            <div
              class="description-item modern-card"
              v-for="desc in displayedDescriptions(img.descriptions, img.id)"
              :key="desc.id"
            >
              <div class="description-header">
                <div class="description-meta">
                  <p class="description-timestamp">
                    <i class="bi bi-clock"></i> {{ formatDate(desc.created) }}
                  </p>
                  <div class="description-objects-tags">
                    <span 
                      class="object-tag"
                      v-for="obj in desc.objects"
                      :key="obj.id"
                    >
                      {{ obj.name }}
                    </span>
                  </div>
                </div>
                <div class="description-actions">
                  <button class="action-btn download-text-btn" @click="downloadDescriptionAsText(img, desc)" title="Télécharger en texte">
                    <i class="bi bi-download"></i>
                  </button>
                  <button class="action-btn voice-btn" @click="speakDescription(desc.text)" title="Écouter">
                    <i class="bi bi-volume-up"></i>
                  </button>
                  <button class="delete-btn modern-icon-btn" @click="deleteDescription(img.id, desc.id)">
                    <i class="bi bi-trash"></i>
                  </button>
                </div>
              </div>
              <div class="description-text-container">
                <p class="description-text">{{ desc.text }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <!-- Image modal -->
  <div v-if="selectedImage" class="image-modal" @click="closeModal">
    <div class="modal-content" @click.stop>
      <button class="close-btn" @click="closeModal">
        <i class="bi bi-x-lg"></i>
      </button>
      <img :src="selectedImage" alt="Image en taille réelle" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import axios from 'axios'
import Cookies from 'js-cookie'

import { useDocumentStore } from '@/Store/analysis.js'

const props = defineProps({
  document: {
    type: Object,
    required: true
  },
  images: {
    type: Array,
    required: true
  }
})

const emit = defineEmits(['show-full-image', 'analyse-document'])

const documentStore = useDocumentStore()

const enrichedImages = ref(props.images.map(img => ({ 
  ...img, 
  objects: [], 
  descriptions: [] 
})))
const selectedObjects = ref({})
const showDescriptions = ref({})
const timers = ref({})
const timerIntervals = ref({})
const showLightbox = ref(false)
const currentImage = ref('')
const isAnalyzing = computed(() => documentStore.isAnalysing)
const selectedImage = ref(null)

// Initialisation
enrichedImages.value.forEach(img => {
  selectedObjects.value[img.id] = []
  showDescriptions.value[img.id] = false
  timers.value[img.id] = 0
})

const getMaxPercentageObjects = (objects) => {
  if (!objects || objects.length === 0) return [];
  
  const maxPercentage = Math.max(...objects.map(o => o.pourcentage || 0));
  return objects.filter(obj => obj.pourcentage === maxPercentage);
};

const openLightbox = (imgUrl) => {
  currentImage.value = imgUrl
  showLightbox.value = true
};

const getPercentageClass = (pourcentage) => {
  if (pourcentage > 70) return 'high-percentage';
  if (pourcentage > 40) return 'medium-percentage';
  return 'low-percentage';
};

const fetchObjects = async () => {
  for (const img of enrichedImages.value) {
    try {
      const response = await axios.get(`/api/images/objects/${img.id}`);
      img.objects = response.data;
      
      // Trouver tous les objets avec le pourcentage maximum
      const maxObjs = getMaxPercentageObjects(response.data);
      
      // Cocher tous les objets ex-aequo avec le pourcentage max
      selectedObjects.value[img.id] = maxObjs.map(obj => obj.id);
    } catch (error) {
      console.error(`Erreur récupération objets image ${img.id}:`, error);
      img.objects = [];
    }
  }
};

const fetchDescriptions = async () => {
  for (const img of enrichedImages.value) {
    try {
      const response = await axios.get(`/api/images/descriptions/${img.id}`)
      img.descriptions = (response.data.descriptions || []).map(d => ({
        ...d,
        text: d.text.replace(/\\n/g, '\n')
      }))
    } catch (error) {
      console.error(`Erreur récupération descriptions image ${img.id}:`, error)
      img.descriptions = []
    }
  }
}

const hasSelectedObjects = imageId => {
  return selectedObjects.value[imageId]?.length > 0
}

const updateSelectedObjects = imageId => {
  if (showDescriptions.value[imageId]) {
    showDescriptions.value[imageId] = false
    setTimeout(() => {
      showDescriptions.value[imageId] = true
    }, 0)
  }
}

const toggleDescriptions = imageId => {
  showDescriptions.value[imageId] = !showDescriptions.value[imageId]
}

const displayedDescriptions = (descriptions, imageId) => {
  if (!descriptions) return []
  
  const selectedIds = selectedObjects.value[imageId] || []
  if (selectedIds.length === 0) return [...descriptions].sort((a, b) => 
    new Date(b.created) - new Date(a.created)
  )

  return descriptions
    .filter(d => d.objects?.some(o => selectedIds.includes(o.id)))
    .sort((a, b) => new Date(b.created) - new Date(a.created))
}

const describeObjects = async imageId => {
  const objectIds = selectedObjects.value[imageId]
  if (!objectIds?.length) {
    alert('Veuillez sélectionner au moins un objet')
    return
  }

  const image = enrichedImages.value.find(img => img.id === imageId)
  const selectedObjs = image.objects.filter(o => objectIds.includes(o.id))

  // Début de l'analyse
  timers.value[imageId] = 0
  timerIntervals.value[imageId] = setInterval(() => timers.value[imageId]++, 1000)
  documentStore.startAnalysing('description', image.name || `Image ${imageId}`)

  try {
    const response = await axios.post(`/api/images/describe/${imageId}`, {
      ImageUrl: image.fileUrl,
      Objects: selectedObjs.map(o => o.name)
    })

    const descText = response.data.Description || response.data.description
    if (!descText) throw new Error('Description vide')

    await fetchDescriptions()
    showDescriptions.value[imageId] = true

  } catch (error) {
    console.error('Erreur description:', error)
    alert(`Échec: ${error.response?.data?.message || error.message}`)
  } finally {
    clearInterval(timerIntervals.value[imageId])
    timers.value[imageId] = 0
    documentStore.stopAnalysing()
  }
}

const deleteDescription = async (imageId, descId) => {
  if (!confirm('Supprimer cette description ?')) return

  try {
    await axios.delete(`/api/descriptions/${descId}`)
    await fetchDescriptions()
  } catch (error) {
    console.error('Erreur suppression:', error)
    alert('Échec de la suppression')
  }
}

const formatDate = dateString => {
  return new Date(dateString).toLocaleString('fr-FR')
}

onMounted(async () => {
  await fetchObjects()
  await fetchDescriptions()
   
  // S'assurer que les objets avec le pourcentage max sont cochés
  enrichedImages.value.forEach(img => {
    if (img.objects && img.objects.length > 0) {
      const maxObjs = getMaxPercentageObjects(img.objects);
      selectedObjects.value[img.id] = maxObjs.map(obj => obj.id);
    }
  });
})

onUnmounted(() => {
  Object.values(timerIntervals.value).forEach(clearInterval)
})

// Fonction pour télécharger la description au format texte
const downloadDescriptionAsText = (img, desc) => {
  try {
    // Créer le contenu du fichier texte
    const dateStr = new Date(desc.created).toLocaleString('fr-FR');
    const objectsStr = desc.objects.map(obj => obj.name).join(', ');
    
    // Formater le contenu avec des informations structurées
    const content = `
Date: ${dateStr}
Image URL: ${img.fileUrl}
Objets identifiés: ${objectsStr}

DESCRIPTION:
${desc.text}

---
Généré par DocVision
`;
    
    // Créer un Blob avec le contenu
    const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
    
    // Créer une URL pour le Blob
    const url = URL.createObjectURL(blob);
    
    // Créer un élément de lien pour le téléchargement
    const link = document.createElement('a');
    link.href = url;
    link.download = `description_${img.id}_${desc.id}.txt`;
    
    // Ajouter le lien au document, cliquer dessus, puis le supprimer
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    // Libérer l'URL
    URL.revokeObjectURL(url);
  } catch (error) {
    console.error('Erreur lors du téléchargement du fichier texte:', error);
    alert('Erreur lors du téléchargement de la description');
  }
};

// Fonction pour lire la description à haute voix
const speakDescription = (text) => {
  if (!window.speechSynthesis) {
    alert('La synthèse vocale n\'est pas prise en charge par votre navigateur');
    return;
  }
  window.speechSynthesis.cancel(); // Arrêter toute lecture en cours
  
  const utterance = new SpeechSynthesisUtterance(text);
  utterance.lang = 'fr-FR';
  
  window.speechSynthesis.speak(utterance);
};

// Fonction pour télécharger l'image
const downloadImage = (imageUrl) => {
  try {
    // Créer un élément a pour le téléchargement
    const link = document.createElement('a');
    link.href = imageUrl;
    
    // Extraire le nom de fichier de l'URL ou utiliser un nom par défaut
    const fileName = imageUrl.split('/').pop().split('?')[0] || 'image.jpg';
    
    const token = Cookies.get('token');
    
    // Utiliser axios pour obtenir le blob avec l'authentification si nécessaire
    axios({
      url: imageUrl,
      method: 'GET',
      responseType: 'blob',
      headers: token ? { Authorization: `Bearer ${token}` } : {}
    })
    .then(response => {
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    });
  } catch (error) {
    console.error('Erreur lors du téléchargement de l\'image:', error);
    alert('Erreur lors du téléchargement de l\'image');
  }
};

// Fonction pour afficher l'image en plein écran
const showFullImage = (imageUrl) => {
  selectedImage.value = imageUrl
}

// Fonction pour fermer la modal
const closeModal = () => {
  selectedImage.value = null
}
</script>

<style scoped>
.analyse-tab {
  padding: 20px;
}

.global-analysis-controls {
  margin-bottom: 2rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
  text-align: center;
}

.analyse-document-btn {
  background: #4a6baf;
  color: white;
  padding: 12px 24px;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.analyse-document-btn:hover:not(:disabled) {
  background: #3a5a9f;
  transform: translateY(-2px);
}

.analyse-document-btn:disabled {
  background: #cccccc;
  cursor: not-allowed;
}

.analysis-warning {
  margin-top: 1rem;
  color: #d32f2f;
  font-size: 0.9rem;
}

.spin-icon {
  animation: spin 1s linear infinite;
}

.images-section {
  margin-top: 20px;
}

.images-list {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

/* Structure principale */
.image-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.08);
  overflow: hidden;
  margin-bottom: 2rem;
}

.image-content-wrapper {
  display: flex;
  flex-direction: row;
  gap: 1.5rem;
  padding: 1.5rem;
}

/* Colonne image */
.image-column {
  flex: 1;
  min-width: 300px;
}

.image-container {
  position: relative;
  height: 400px;
  background: #f8fafc;
  border-radius: 8px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.image-container img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
}

.image-actions {
  position: absolute;
  top: 10px;
  right: 10px;
  display: flex;
  gap: 8px;
}

.zoom-btn, .download-img-btn {
  background-color: rgba(255, 255, 255, 0.9);
  border: none;
  border-radius: 50%;
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
}

.zoom-btn:hover, .download-img-btn:hover {
  background-color: white;
  transform: scale(1.1);
}

.zoom-btn i, .download-img-btn i {
  font-size: 1.2rem;
  color: #333;
}

/* Colonne tableau */
.table-column {
  flex: 1;
  min-width: 300px;
  display: flex;
  flex-direction: column;
}

.modern-table-container {
  flex: 1;
}

.table-title {
  font-size: 1.1rem;
  color: #334155;
  margin-bottom: 1rem;
  font-weight: 600;
}

.modern-table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
}

.modern-table th {
  background: #f1f5f9;
  color: #475569;
  font-weight: 600;
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  padding: 12px 15px;
  text-align: left;
}

.modern-table td {
  padding: 12px 15px;
  border-bottom: 1px solid #f1f5f9;
  font-size: 0.9rem;
}

.modern-table tr:last-child td {
  border-bottom: none;
}

/* Checkbox stylisée */
.checkbox-container {
  display: block;
  position: relative;
  cursor: pointer;
  user-select: none;
}

.checkbox-container input {
  position: absolute;
  opacity: 0;
  cursor: pointer;
  height: 0;
  width: 0;
}

.checkmark {
  position: relative;
  height: 18px;
  width: 18px;
  background-color: white;
  border: 2px solid #cbd5e1;
  border-radius: 4px;
  transition: all 0.2s;
}

.checkbox-container:hover input ~ .checkmark {
  border-color: #94a3b8;
}

.checkbox-container input:checked ~ .checkmark {
  background-color: #3b82f6;
  border-color: #3b82f6;
}

.checkmark:after {
  content: "";
  position: absolute;
  display: none;
}

.checkbox-container input:checked ~ .checkmark:after {
  display: block;
}

.checkbox-container .checkmark:after {
  left: 5px;
  top: 1px;
  width: 5px;
  height: 10px;
  border: solid white;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

/* Badge de pourcentage */
.percentage-badge {
  display: inline-block;
  padding: 3px 8px;
  border-radius: 10px;
  font-size: 0.8rem;
  font-weight: 500;
}

.high-percentage {
  background: #dcfce7;
  color: #166534;
}

.medium-percentage {
  background: #fef9c3;
  color: #854d0e;
}

.low-percentage {
  background: #fee2e2;
  color: #991b1b;
}

/* Boutons d'action */
.action-buttons {
  display: flex;
  gap: 12px;
  margin-top: 1.5rem;
}

.modern-action-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  border: none;
}

.modern-action-btn.primary {
  background: #3b82f6;
  color: white;
}

.modern-action-btn.primary:hover {
  background: #2563eb;
}

.modern-action-btn.primary:disabled {
  background: #cbd5e1;
  cursor: not-allowed;
}

.modern-action-btn.secondary {
  background: white;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.modern-action-btn.secondary:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
}

/* Section descriptions (en dessous) */
.modern-descriptions {
  border-top: 1px solid #f1f5f9;
  padding: 1.5rem;
  background: #f8fafc;
}

/* Bouton Actualiser modernisé */
.refresh-btn.modern-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  color: #64748b;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}

.refresh-btn.modern-btn:hover {
  background: #f1f5f9;
  color: #475569;
  border-color: #cbd5e1;
}

.refresh-btn.modern-btn i {
  font-size: 0.9rem;
}

/* Carte de description modernisée */
.description-item.modern-card {
  background: white;
  padding: 1.5rem;
  border-radius: 10px;
  margin-bottom: 1.25rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
  border: 1px solid #e2e8f0;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.description-item.modern-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 6px rgba(0,0,0,0.05);
}

.description-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.description-meta {
  flex: 1;
}

.description-timestamp {
  font-size: 0.8rem;
  color: #64748b;
  margin-bottom: 0.75rem;
  display: flex;
  align-items: center;
  gap: 0.3rem;
}

.description-objects-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.object-tag {
  background: #f1f5f9;
  color: #334155;
  padding: 0.25rem 0.75rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 500;
  display: inline-flex;
  align-items: center;
}

/* Bouton de suppression modernisé */
.delete-btn.modern-icon-btn {
  background: transparent;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: all 0.2s ease;
}

.delete-btn.modern-icon-btn:hover {
  background: #fee2e2;
  color: #dc2626;
}

.description-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  background: transparent;
  border: none;
  color: #64748b;
  cursor: pointer;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: all 0.2s ease;
}

.action-btn:hover {
  background: #f1f5f9;
  color: #3b82f6;
}

.download-text-btn:hover {
  background: #e0f2fe;
  color: #0284c7;
}

.voice-btn:hover {
  background: #ecfdf5;
  color: #059669;
}

.description-text-container {
  padding: 0.75rem;
  background: #f8fafc;
  border-radius: 8px;
  border-left: 3px solid #3b82f6;
}

.description-text {
  color: #334155;
  line-height: 1.6;
  white-space: pre-line;
  margin: 0;
}

.no-descriptions {
  text-align: center;
  padding: 1.5rem;
  color: #94a3b8;
  font-style: italic;
  background: white;
  border-radius: 8px;
  border: 1px dashed #e2e8f0;
}
/* Style personnalisé pour les checkbox */
.custom-checkbox {
  display: block;
  position: relative;
  padding-left: 28px;
  cursor: pointer;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  user-select: none;
  height: 20px;
}

.custom-checkbox input {
  position: absolute;
  opacity: 0;
  cursor: pointer;
  height: 0;
  width: 0;
}

.checkmark {
  position: absolute;
  top: 0;
  left: 0;
  height: 20px;
  width: 20px;
  background-color: white;
  border: 2px solid #cbd5e1;
  border-radius: 4px;
  transition: all 0.2s ease;
}

.custom-checkbox:hover input ~ .checkmark {
  border-color: #94a3b8;
  background-color: #f8fafc;
}

.custom-checkbox input:checked ~ .checkmark {
  background-color: #4F46E5;
  border-color: #4F46E5;
}

.checkmark:after {
  content: "";
  position: absolute;
  display: none;
}

.custom-checkbox input:checked ~ .checkmark:after {
  display: block;
}

.custom-checkbox .checkmark:after {
  left: 6px;
  top: 2px;
  width: 5px;
  height: 10px;
  border: solid white;
  border-width: 0 2px 2px 0;
  -webkit-transform: rotate(45deg);
  -ms-transform: rotate(45deg);
  transform: rotate(45deg);
}

/* Alignement vertical des cases à cocher */
.modern-table td:first-child {
  vertical-align: middle;
  text-align: center;
}


.no-images {
  text-align: center;
  color: #6c757d;
  font-style: italic;
  padding: 20px;
}

.spinner {
  display: inline-block;
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255,255,255,0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 1s linear infinite;
  margin-right: 8px;
}

.no-objects-message {
  text-align: center;
  padding: 2rem;
  color: #94a3b8;
  font-style: italic;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 768px) {
  .image-content-wrapper {
    flex-direction: column;
  }
  
  .image-column,
  .table-column {
    min-width: 100%;
  }
}

/* Image modal styles */
.image-modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.9);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 20px;
}

.modal-content {
  position: relative;
  max-width: 90%;
  max-height: 90vh;
  background-color: transparent;
}

.modal-content img {
  max-width: 100%;
  max-height: 90vh;
  object-fit: contain;
  border-radius: 8px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
}

.close-btn {
  position: absolute;
  top: -40px;
  right: 0;
  background-color: transparent;
  border: none;
  color: white;
  font-size: 24px;
  cursor: pointer;
  transition: transform 0.2s;
}

.close-btn:hover {
  transform: scale(1.2);
}
</style>
