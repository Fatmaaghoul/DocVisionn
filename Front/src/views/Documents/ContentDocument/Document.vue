<template>
  <div class="container">
    <!-- Main content layout -->
    <div class="content-layout">
      <!-- Left half: buttons and dynamic content -->
      <div class="left-content">
        <!-- Horizontal button bar at the top -->
        <div class="button-bar">
          <button
            class="content-btn"
            :class="{ active: activeSection === 'images' }"
            @click="activeSection = 'images'"
          >
            Images
          </button>
          <button
            class="content-btn"
            :class="{ active: activeSection === 'résumé' }"
            @click="activeSection = 'résumé'"
          >
            Résumé
          </button>
          <button
            class="content-btn"
            :class="{ active: activeSection === 'information' }"
            @click="activeSection = 'information'"
          >
            Information
          </button>
          <button
            class="content-btn"
            :class="{ active: activeSection === 'text' }"
            @click="activeSection = 'text'"
          >
            Texte
          </button>
          <!-- Analyse button with spinning icon -->
          <button
            class="analyse-btn"
            @click="analyseDocument"
            :disabled="documentStore.isAnalysing || document.isAnalysed"
          >
            <i class="bi bi-gear" :class="{ 'spin-icon': documentStore.isAnalysing }"></i> Analyser
          </button>
        </div>

        <!-- Dynamic content area -->
        <div class="dynamic-content">
          <div v-if="isLoadingImages && activeSection === 'images'" class="loading">
            <p>Chargement des images...</p>
          </div>
          <Information
            v-if="activeSection === 'information'"
            :document="document"
            @update-document="updateDocument"
            @analyse-document="analyseDocument"
          />
          <Images
            v-if="activeSection === 'images' && !isLoadingImages"
            :images="images"
            @show-full-image="showFullImage"
          />
          <Text
            v-if="activeSection === 'text' && document.text"
            :text="document.text"
            @copy-text="copyText"
          />
          <Résumé
            v-if="activeSection === 'résumé' && !isExtracting"
            :resumer="document.resumer"
            :document-id="documentId"
            :document="document"
            @update-resumer="updateResumer"
          />
        </div>
      </div>

      <!-- Right preview -->
      <Preview :document="document" />
    </div>

    <!-- Loading bar at the bottom with incrementing timer -->
    <!-- <div v-if="isExtracting || documentStore.isAnalysing" class="loading-bar">
      <div class="loading-progress"></div>
      <p>
        {{ isExtracting ? 'Extraction en cours...' : 'Analyse en cours...' }}
        <span>({{ timer }}s écoulées)</span>
      </p>
    </div> -->

    <!-- Image modal -->
    <div v-if="selectedImage" class="image-modal" @click="closeModal">
      <div class="modal-content" @click.stop>
        <button class="close-btn" @click="closeModal">
          <i class="bi bi-x-lg"></i>
        </button>
        <img :src="selectedImage" alt="Image en taille réelle" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, onUnmounted } from 'vue';
import { useRoute } from 'vue-router';
import axios from 'axios';
import { useDocumentStore } from '@/Store/analysis';
import Preview from './Preview.vue';
import Information from './Information.vue';
import Images from './Images.vue';
import Text from './Text.vue';
import Résumé from './Resumer.vue';

const route = useRoute();
const documentId = route.params.id;
const selectedImage = ref(null);
const isCopied = ref(false);
const documentStore = useDocumentStore();
const activeSection = ref('images'); // Default active section to images
const isLoadingImages = ref(true); // Loading state for images

const document = ref({
  name: '',
  description: '',
  isExtracted: false,
  isAnalysed: false,
  uploadDate: null,
  fileUrl: null,
  text: null,
  resumer: null,
});
const images = ref([]);
const isExtracting = ref(false);
const timer = ref(0); // Timer in seconds (incrementing)
let timerInterval = null; // Interval for timer increment

// Start the timer
const startTimer = () => {
  if (timerInterval) clearInterval(timerInterval); // Clear any existing interval
  timer.value = 0;
  timerInterval = setInterval(() => {
    timer.value += 1;
  }, 1000);
};

// Stop and reset the timer
const stopTimer = () => {
  if (timerInterval) {
    clearInterval(timerInterval);
    timerInterval = null;
  }
  timer.value = 0;
};

// Watch for changes in isExtracting or isAnalysing to control the timer
watch([isExtracting, () => documentStore.isAnalysing], ([newExtracting, newAnalysing]) => {
  if (newExtracting || newAnalysing) {
    startTimer();
  } else {
    stopTimer();
  }
});

const fetchDocument = async () => {
  try {
    const res = await axios.get(`api/documents/${documentId}`);
    document.value = res.data;
    console.log(res.data);
    if (!document.value.isExtracted) {
      await extractDocumentContent();
    } else {
      await fetchImages();
    }
  } catch (error) {
    console.error('Erreur lors de la récupération du document :', error);
    alert('❌ Erreur lors de la récupération du document');
  }
};

const fetchImages = async () => {
  try {
    isLoadingImages.value = true;
    const res = await axios.get(`api/images/${documentId}`);
    images.value = res.data;
  } catch (error) {
    console.error('Erreur lors de la récupération des images :', error);
  } finally {
    isLoadingImages.value = false;
  }
};

const extractDocumentContent = async () => {
  try {
    isExtracting.value = true
    documentStore.startExtracting(document.value.name || '')
    
    if (!document.value.isExtracted) {
      await axios.post(`api/documents/extract/${documentId}`)
      document.value.isExtracted = true
      const res = await axios.get(`api/documents/${documentId}`)
      document.value = res.data
      await fetchImages()
    }
  } catch (error) {
    console.error('Erreur lors de l\'extraction du contenu :', error)
    alert('❌ Erreur lors de l\'extraction du contenu')
  } finally {
    isExtracting.value = false
    documentStore.stopExtracting()
  }
}

const updateDocument = async updatedData => {
  try {
    await axios.put(`api/documents/${documentId}`, updatedData);
    document.value.name = updatedData.name;
    document.value.description = updatedData.description;
    alert('Document mis à jour avec succès ✅');
  } catch (error) {
    console.error('Erreur lors de la mise à jour du document :', error);
    alert('❌ Échec de la mise à jour');
  }
};

const analyseDocument = async () => {
  try {
    documentStore.startAnalysing('analyse', document.value.name);
    const response = await axios.post(`api/documents/analyze/${documentId}`);
    const res = await axios.get(`api/documents/${documentId}`);
    document.value = res.data;
    await fetchImages(); // Refresh images to include updated objects
    alert('Document analysé avec succès ✅');
  } catch (error) {
    console.error('Erreur lors de l\'analyse du document :', error);
    alert(`❌ Échec de l'analyse : ${error.response?.data?.message || error.message}`);
  } finally {
    documentStore.stopAnalysing();
  }
};

const updateResumer = newResumer => {
  document.value.resumer = newResumer; // Mise à jour du résumé
};

const copyText = () => {
  navigator.clipboard.writeText(document.value.text).then(() => {
    isCopied.value = true;
    setTimeout(() => {
      isCopied.value = false;
    }, 2000);
  });
};

const showFullImage = imageUrl => {
  selectedImage.value = imageUrl;
};

const closeModal = () => {
  selectedImage.value = null;
};

// Cleanup on component unmount
onUnmounted(() => {
  stopTimer(); // Clear timer interval to prevent memory leaks
});

onMounted(() => {
  fetchDocument();
});
</script>

<style scoped>
.container {
  max-width: 100%;
  margin-top: 42px; /* Preserved original margin */
  padding: 10px;
  font-family: Arial, sans-serif;
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.loading-bar {
  position: fixed;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 48px;
  background: #e2e8f0; /* Grey rectangle */
  color: #1e293b;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  z-index: 1000;
  box-shadow: 0 -2px 4px rgba(0, 0, 0, 0.1);
  font-size: 0.95rem;
  font-weight: 500;
}

.loading-bar p {
  margin: 0;
  display: flex;
  align-items: center;
  gap: 5px;
}

.loading-bar span {
  font-size: 0.9rem;
  color: #4b5563; /* Slightly lighter color for timer */
}

.loading-progress {
  position: absolute;
  top: 0;
  left: 0;
  height: 4px;
  width: 100%;
  background: linear-gradient(to right, #3b82f6 0%, #3b82f6 50%, transparent 50%, transparent 100%);
  background-size: 200% 100%;
  animation: progress 2s linear infinite;
}

@keyframes progress {
  0% {
    background-position: 200% 0;
  }
  100% {
    background-position: 0 0;
  }
}

.content-layout {
  display: flex;
  flex: 1;
  gap: 0;
}

.left-content {
  margin-top: 25px;
  width: 50%;
  display: flex;
  flex-direction: column;
}

.button-bar {
  background-color: rgb(255, 255, 255);
  z-index: 99;
  position: fixed;
  display: flex;
  gap: 20px;
  padding: 10px;
  width: calc(50% - 20px);
}

.content-btn {
  background: none;
  border: none;
  padding: 5px;
  cursor: pointer;
  position: relative;
  font-size: 16px;
  color: #5f6368;
  transition: color 0.3s;
}

.content-btn.active::after {
  content: '';
  position: absolute;
  bottom: -2px;
  left: 0;
  width: 100%;
  height: 2px;
  background-color: #1a73e8;
}

.content-btn:hover {
  color: #1a73e8;
}

.analyse-btn {
  margin-left: auto;
  background-color: #1bc0c8;
  color: white;
  padding: 8px 16px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 16px;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: background-color 0.3s;
}

.analyse-btn:hover {
  background-color: #17a2b8;
}

.analyse-btn:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
}

.spin-icon {
  animation: spin 1s ease-in-out infinite;
}

.dynamic-content {
  margin-top: 50px;
  flex: 1;
  padding: 20px;
  overflow-y: auto;
}

.loading {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100%;
  color: #6c757d;
  font-style: italic;
  font-size: 1rem;
}

.preview {
  position: fixed;
  right: 0;
  top: 0;
  width: 50%;
  height: 100vh;
  margin-right: 0;
  background-color: #fff;
  border-left: 1px solid #ddd;
  overflow-y: auto;
  padding: 20px;
  box-sizing: border-box;
}

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
  right: -40px;
  background-color: white;
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

.close-btn:hover {
  transform: scale(1.1);
  background-color: #f8f9fa;
}

.close-btn i {
  font-size: 1.2rem;
  color: #333;
}

@media (max-width: 992px) {
  .content-layout {
    flex-direction: column;
  }

  .left-content {
    width: 100%;
  }

  .button-bar {
    flex-wrap: wrap;
    justify-content: center;
    width: 100%;
  }

  .analyse-btn {
    margin-left: 0;
    margin-top: 10px;
  }

  .dynamic-content {
    padding: 15px;
  }

  .preview {
    position: static;
    width: 100%;
    height: auto;
    border-left: none;
    border-top: 1px solid #ddd;
  }
}

@media (max-width: 768px) {
  .container {
    padding: 15px;
  }

  .button-bar {
    flex-direction: row;
    justify-content: center;
    gap: 15px;
  }

  .content-btn {
    padding: 5px;
  }

  .modal-content {
    max-width: 95%;
  }

  .close-btn {
    top: -30px;
    right: 0;
  }
}
</style>

