<template>
  <div class="document-info">
    <div class="info-card">
      <div class="form-group">
        <label>Nom :</label>
        <input v-model="tempDocument.name" class="form-input" placeholder="Entrez le nom" />
      </div>

      <div class="form-group">
        <label>Description :</label>
        <textarea
          v-model="tempDocument.description"
          class="form-textarea"
          placeholder="Entrez la description"
        ></textarea>
      </div>

      <div class="form-group">
        <label>Date de création :</label>
        <p class="info-text">{{ formatDate(document.uploadDate) }}</p>
      </div>

      <div class="form-group">
        <label>Propriétaire :</label>
        <p class="info-text">{{ ownerName || 'Chargement...' }}</p>
      </div>

      <div class="form-group">
        <label>Statut :</label>
        <p class="info-text">
          <span :class="['status-badge', document.isExtracted ? 'extracted' : 'pending']">
            {{ document.isExtracted ? 'Extrait' : 'En attente d\'extraction' }}
          </span>
          <span :class="['status-badge', document.isAnalysed ? 'traiter' : 'non-traiter']">
            {{ document.isAnalysed ? 'Analysée' : 'Non analysée' }}
          </span>
        </p>
      </div>

      <div class="form-group">
        <label>Objets :</label>
        <p class="info-text">
          <span
            v-for="obj in objects"
            :key="obj.id"
            class="status-badge object-tag"
          >
            {{ obj.name || 'Objet sans nom' }}
          </span>
          <span v-if="objects.length === 0" class="no-objects">
            Aucun objet détecté
          </span>
        </p>
      </div>

      <div class="button-group">
        <button @click="submitUpdate" class="update-btn">
          <i class="bi bi-save"></i> Mis à jour
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useDocumentStore } from '@/Store/analysis'
import axios from 'axios'

const props = defineProps({
  document: {
    type: Object,
    required: true
  }
})

const emit = defineEmits(['update-document', 'analyse-document'])

const documentStore = useDocumentStore()
const objects = ref([])
const ownerName = ref('') // Variable pour stocker le nom du propriétaire

const tempDocument = ref({
  name: props.document.name || '',
  description: props.document.description || ''
})

const formatDate = (dateString) => {
  if (!dateString) return 'Non disponible'
  const date = new Date(dateString)
  return date.toLocaleDateString('fr-FR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const fetchObjects = async () => {
  try {
    const response = await axios.get(`/api/object/by-document/${props.document.id}`)
    objects.value = response.data
  } catch (error) {
    console.error('Erreur lors de la récupération des objets:', error)
    objects.value = []
  }
}

const fetchOwner = async () => {
  try {
    console.log(props.document.propriétaireId);
    if (props.document.propriétaireId) {
      const response = await axios.get('/api/users/id', {
        params: { id: props.document.propriétaireId }
      })
      console.log(response.data);
      ownerName.value = response.data.userName || 'Propriétaire inconnu' // Ajusté selon la propriété userName
    } else {
      ownerName.value = 'Aucun propriétaire'
    }
  } catch (error) {
    console.error('Erreur lors de la récupération du propriétaire:', error)
    ownerName.value = 'Erreur de chargement'
  }
}

const submitUpdate = () => {
  emit('update-document', {
    name: tempDocument.value.name,
    description: tempDocument.value.description
  })
}

onMounted(() => {
  fetchObjects()
  fetchOwner() // Charger le propriétaire au montage du composant
})
</script>

<style scoped>
.document-info {
  flex: 1;
  min-width: 0;
}

.info-card {
  background: #fefefe;
  padding: 20px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #333;
}

.form-input,
.form-textarea {
  width: 100%;
  padding: 12px;
  border-radius: 8px;
  border: 1px solid #ddd;
  font-size: 16px;
  transition: border-color 0.3s;
}

.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: #4caf50;
}

.form-textarea {
  resize: vertical;
  min-height: 100px;
}

.info-text {
  margin: 0;
  padding: 12px;
  background-color: #f8f9fa;
  border-radius: 8px;
  color: #333;
  word-break: break-word;
}

.status-badge {
  display: inline-block;
  padding: 5px 10px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 600;
  margin-right: 8px;
}

.status-badge.extracted {
  background-color: #d4edda;
  color: #155724;
}

.status-badge.pending {
  background-color: #fff3cd;
  color: #856404;
}

.status-badge.traiter {
  background-color: #cce5ff;
  color: #004085;
}

.status-badge.non-traiter {
  background-color: #f8d7da;
  color: #721c24;
}

.status-badge.object-tag {
  background-color: #cce5ff;
  color: #004085;
}

.no-objects {
  color: #6c757d;
  font-size: 14px;
}

.button-group {
  display: flex;
  justify-content: flex-end;
}

.update-btn {
  padding: 10px 20px;
  border-radius: 8px;
  border: none;
  background-color: #4caf50;
  color: white;
  font-size: 16px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.update-btn:hover { 
  background-color: #45a049;
}

.update-btn:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
}
</style>