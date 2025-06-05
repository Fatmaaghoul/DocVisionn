<template>
  <div class="parametre-container">
    <div class="model-card">
      <h2>Ajouter un modèle</h2>
      <div class="input-group">
        <input v-model="modelName" placeholder="Nom du modèle" class="form-control" />
        <button @click="handleClick" :disabled="!modelName.trim() && !isDownloading" class="btn-primary">
          {{ isDownloading ? 'Annuler' : 'Ajouter un modèle' }}
        </button>
      </div>

      <div v-if="isDownloading" class="download-status">
        <p>Téléchargement: {{ progress }}% - {{ message }}</p>
        <progress :value="progress" max="100"></progress>
      </div>
    </div>

    <div class="model-list-card">
      <h2>Liste des modèles</h2>
      <ul class="models-list">
        <li v-for="model in models" :key="model.id" class="model-item">
          <div class="model-name">{{ model.name }}</div>
          <button 
            @click="activateModel(model.id)" 
            :disabled="model.isActive"
            :class="['btn-action', model.isActive ? 'btn-active' : '']"
          >
            {{ model.isActive ? 'Actif' : 'Activer' }}
          </button>
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import axios from 'axios'

const modelName = ref('')
const isDownloading = ref(localStorage.getItem('isDownloading') === 'true')
const progress = ref(Number(localStorage.getItem('progress')) || 0)
const message = ref(localStorage.getItem('message') || '')
const models = ref([])

let pollingInterval = null

// Mettre à jour localStorage à chaque changement d'état
function updateLocalStorage() {
  localStorage.setItem('isDownloading', isDownloading.value)
  localStorage.setItem('progress', progress.value)
  localStorage.setItem('message', message.value)
}

async function fetchModels() {
  try {
    const response = await axios.get('/api/modelai')
    models.value = response.data
  } catch (error) {
    alert('Erreur lors du chargement des modèles')
  }
}

async function activateModel(id) {
  try {
    await axios.post(`/api/modelai/set-active/${id}`)
    alert('Modèle activé avec succès')
    await fetchModels()
  } catch (error) {
    alert('Erreur lors de l\'activation du modèle')
  }
}

async function startDownload() {
  try {
    const response = await axios.post('/api/modelai/download', { model_name: modelName.value })
    isDownloading.value = true
    message.value = response.data.message || 'Téléchargement démarré'
    progress.value = 0
    updateLocalStorage()
    modelName.value = '' // Réinitialiser le champ
    startPolling()
  } catch (error) {
    alert(error.response?.data?.message || 'Erreur lors du démarrage du téléchargement')
  }
}

async function cancelDownload() {
  try {
    const response = await axios.post('/api/modelai/cancel')
    message.value = response.data.message || 'Téléchargement annulé'
    stopPolling()
    resetState()
    await fetchModels()
  } catch (error) {
    alert(error.response?.data?.message || 'Erreur lors de l\'annulation')
  }
}

async function checkStatus() {
  try {
    const response = await axios.get('/api/modelai/download-status')
    const data = response.data
    if (data && data.status) {
      isDownloading.value = ['pending', 'downloading'].includes(data.status)
      progress.value = data.progress || 0
      message.value = data.message || ''
      updateLocalStorage()
      if (['completed', 'error', 'cancelled'].includes(data.status)) {
        stopPolling()
        isDownloading.value = false
        updateLocalStorage()
        if (data.status === 'completed') {
          alert('Téléchargement terminé avec succès !')
          await fetchModels()
        } else if (data.status === 'error') {
          alert('Erreur pendant le téléchargement : ' + message.value)
        } 
      }
    } else {
      isDownloading.value = false
      progress.value = 0
      message.value = ''
      updateLocalStorage()
    }
  } catch (error) {
    stopPolling()
    isDownloading.value = false
    updateLocalStorage()
    alert('Erreur lors de la récupération du statut')
  }
}

function startPolling() {
  if (pollingInterval) clearInterval(pollingInterval)
  pollingInterval = setInterval(checkStatus, 1000)
}

function stopPolling() {
  if (pollingInterval) {
    clearInterval(pollingInterval)
    pollingInterval = null
  }
}

function resetState() {
  isDownloading.value = false
  progress.value = 0
  message.value = ''
  updateLocalStorage()
}

function handleClick() {
  if (isDownloading.value) {
    cancelDownload()
  } else {
    startDownload()
  }
}

onMounted(async () => {
  await fetchModels()
  // Vérifier l'état du téléchargement au montage
  await checkStatus()
  if (isDownloading.value) {
    startPolling()
  }
})

onUnmounted(() => {
  stopPolling()
})
</script>

<style scoped>
.parametre-container {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.model-card, .model-list-card {
  background: var(--background-color);
  border-radius: 10px;
  padding: 2rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid var(--border-color);
}

h2 {
  margin-bottom: 1.5rem;
  color: var(--text-color);
  font-weight: 600;
}

.input-group {
  display: flex;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.form-control {
  flex: 1;
  padding: 0.75rem 1rem;
  border: 1px solid var(--border-color);
  border-radius: 6px;
  font-size: 1rem;
  background-color: var(--background-color);
  color: var(--text-color);
}

.btn-primary {
  background: linear-gradient(135deg, var(--primary-color) 0%, #224abe 100%);
  color: white;
  border: none;
  border-radius: 6px;
  padding: 0.75rem 1.5rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-primary:hover:not(:disabled) {
  opacity: 0.9;
  transform: translateY(-1px);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.download-status {
  background: var(--secondary-background);
  border-radius: 8px;
  padding: 1rem;
  margin-top: 1rem;
}

progress {
  width: 100%;
  height: 10px;
  border-radius: 5px;
  margin-top: 0.5rem;
}

.models-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.model-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-bottom: 1px solid var(--border-color);
  transition: background-color 0.2s;
}

.model-item:last-child {
  border-bottom: none;
}

.model-item:hover {
  background-color: var(--secondary-background);
}

.model-name {
  font-weight: 500;
  color: var(--text-color);
}

.btn-action {
  background-color: var(--secondary-background);
  color: var(--primary-color);
  border: 1px solid var(--primary-color);
  border-radius: 6px;
  padding: 0.5rem 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-action:hover:not(:disabled) {
  background-color: var(--primary-color);
  color: white;
}

.btn-action:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-active {
  background-color: var(--primary-color);
  color: white;
}

@media (max-width: 768px) {
  .parametre-container {
    padding: 1rem;
  }
  
  .input-group {
    flex-direction: column;
  }
  
  .btn-primary {
    width: 100%;
  }
}
</style>
