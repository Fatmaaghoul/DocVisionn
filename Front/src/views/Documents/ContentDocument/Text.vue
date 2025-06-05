<template>
  <div class="resumer-container">
    <!-- Contenu du résumé -->
    <div class="resumer-content">
      <!-- Bouton Copier en haut à droite -->
      <button
        v-if="text && text !== ''"
        class="copy-btn"
        @click="copyResumer"
        title="Copier le Texte"
      >
        <i :class="isCopied ? 'bi bi-check' : 'bi bi-clipboard'"></i>
      </button>
      <p >{{ text }}</p>
      
    </div>

    <!-- Notification de copie -->
    <div v-if="isCopied" class="copy-notification">
      Text copié ! ✅
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'

const props = defineProps({
  text: {
    type: String,
    default: ''
  },
  documentId: {
    type: String,
    required: true
  }
})

const isCopied = ref(false)

const copyResumer = () => {
  if (props.text && props.text !== '') {
    navigator.clipboard.writeText(props.text).then(() => {
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
  background-color: #1bc0c8;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: background-color 0.3s;
  align-self: flex-start; /* Aligner à gauche */
}

.regenerate-btn:hover {
  background-color: #17a2b8;
}

.regenerate-btn:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
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