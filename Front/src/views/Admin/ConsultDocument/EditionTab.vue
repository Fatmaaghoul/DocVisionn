<template>
  <div class="edit-tab">
    <!-- Mode lecture -->
    <div v-if="!isEditing" class="edit-mode">
      <div class="form-group">
        <label>Nom du document</label>
        <p class="form-input-static">{{ document.name }}</p>
      </div>

      <div class="form-group">
        <label>Description</label>
        <pre class="form-textarea-static">{{ document.description || 'Aucune description' }}</pre>
      </div>

      <div class="form-group">
        <label>Date de téléversement</label>
        <p class="form-input-static">{{ formatDate(document.uploadDate) }}</p>
      </div>

      <div class="form-group">
        <label>Propriétaire</label>
        <p class="form-input-static">{{ ownerName || 'Chargement...' }}</p>
      </div>

      <div class="form-group">
        <label>Statut</label>
        <p class="form-input-static">
          <span :class="['status-badge', document.isExtracted ? 'extracted' : 'pending']">
            {{ document.isExtracted ? 'Extrait' : 'En attente d\'extraction' }}
          </span>
          <span :class="['status-badge', document.isAnalysed ? 'traiter' : 'non-traiter']">
            {{ document.isAnalysed ? 'Analysée' : 'Non analysée' }}
          </span>
        </p>
      </div>

      <div class="form-group">
        <label>Objets</label>
        <p class="form-input-static">
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

      <div class="action-buttons">
        <button @click="enableEditing" class="edit-btn">
          <i class="bi bi-pencil"></i> Modifier
        </button>
      </div>
    </div>

    <!-- Mode édition -->
    <div v-else class="edit-mode">
      <div class="form-group">
        <label>Nom du document</label>
        <input v-model="editForm.name" type="text" class="form-input">
      </div>
      
      <div class="form-group">
        <label>Description</label>
        <textarea v-model="editForm.description" class="form-textarea" rows="5"></textarea>
      </div>
      
      <div class="form-group">
        <label>Date de téléversement</label>
        <p class="form-input-static">{{ formatDate(document.uploadDate) }}</p>
      </div>

      <div class="form-group">
        <label>Propriétaire</label>
        <p class="form-input-static">{{ ownerName || 'Chargement...' }}</p>
      </div>

      <div class="form-group">
        <label>Statut</label>
        <p class="form-input-static">
          <span :class="['status-badge', document.isExtracted ? 'extracted' : 'pending']">
            {{ document.isExtracted ? 'Extrait' : 'En attente d\'extraction' }}
          </span>
          <span :class="['status-badge', document.isAnalysed ? 'traiter' : 'non-traiter']">
            {{ document.isAnalysed ? 'Analysée' : 'Non analysée' }}
          </span>
        </p>
      </div>
      
      <div class="action-buttons">
        <button @click="cancelEditing" class="cancel-btn">Annuler</button>
        <button @click="saveChanges" class="save-btn">Enregistrer</button>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import axios from 'axios'

export default {
  props: {
    document: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      isEditing: false,
      editForm: {
        name: '',
        description: ''
      },
      objects: [],
      ownerName: ''
    }
  },
  methods: {
    enableEditing() {
      this.isEditing = true;
      this.editForm = {
        name: this.document.name,
        description: this.document.description
      };
    },
    
    cancelEditing() {
      this.isEditing = false;
    },
    
    saveChanges() {
      this.$emit('update-document', this.editForm);
      this.isEditing = false;
    },
    
    formatDate(dateString) {
      if (!dateString) return 'N/A';
      const date = new Date(dateString);
      return date.toLocaleDateString('fr-FR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    },

    async fetchObjects() {
      try {
        const response = await axios.get(`/api/object/by-document/${this.document.id}`)
        this.objects = response.data
      } catch (error) {
        console.error('Erreur lors de la récupération des objets:', error)
        this.objects = []
      }
    },

    async fetchOwner() {
      try {
        if (this.document.propriétaireId) {
          const response = await axios.get('/api/users/id', {
            params: { id: this.document.propriétaireId }
          })
          this.ownerName = response.data.userName || 'Propriétaire inconnu'
        } else {
          this.ownerName = 'Aucun propriétaire'
        }
      } catch (error) {
        console.error('Erreur lors de la récupération du propriétaire:', error)
        this.ownerName = 'Erreur de chargement'
      }
    }
  },
  mounted() {
    this.fetchObjects()
    this.fetchOwner()
  }
}
</script>

<style scoped>
.edit-tab {
  flex: 1;
  min-width: 0;
}

.edit-mode {
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

.form-input-static,
.form-textarea-static {
  margin: 0;
  padding: 12px;
  background-color: #f8f9fa;
  border-radius: 8px;
  color: #333;
  word-break: break-word;
}

.form-textarea-static {
  white-space: pre-wrap;
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

.action-buttons {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 20px;
}

.edit-btn, .save-btn {
  padding: 10px 20px;
  border-radius: 8px;
  border: none;
  background-color: #4caf50;
  color: white;
  font-size: 16px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.edit-btn:hover, .save-btn:hover { 
  background-color: #45a049;
}

.cancel-btn {
  padding: 10px 20px;
  border-radius: 8px;
  border: none;
  background-color: #f5f5f5;
  color: #405569;
  font-size: 16px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.cancel-btn:hover {
  background-color: #e0e0e0;
}
</style>