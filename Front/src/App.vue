<script>
import Header from '@/components/Header.vue'
import Footer from '@/components/Footer.vue'
import { useThemeStore } from '@/Store/theme'
import { useDocumentStore } from '@/Store/analysis'
import { onMounted, ref, watch, onBeforeUnmount } from 'vue'

export default {
  name: 'App',
  components: {
    Header,
    Footer
  },
  setup() {
    const themeStore = useThemeStore()
    const documentStore = useDocumentStore()
    const timer = ref(parseInt(localStorage.getItem('analysisTimer') || '0'))
    const isExtracting = ref(localStorage.getItem('isExtracting') === 'true')
    let timerInterval = null

    const startTimer = () => {
      timer.value = parseInt(localStorage.getItem('analysisTimer') || '0')
      clearInterval(timerInterval)
      timerInterval = setInterval(() => {
        timer.value += 1
        localStorage.setItem('analysisTimer', timer.value.toString())
      }, 1000)
    }

    const stopTimer = () => {
      clearInterval(timerInterval)
      timerInterval = null
      timer.value = 0
      localStorage.removeItem('analysisTimer')
      localStorage.setItem('isAnalysing', 'false') // Réinitialiser isAnalysing
      localStorage.setItem('isExtracting', 'false') // Réinitialiser isExtracting
    }

    // Initialiser l'état d'analyse depuis localStorage
    if (localStorage.getItem('isAnalysing') === 'true') {
      documentStore.startAnalysing()
    }

    // Surveiller les changements de isAnalysing
    watch(() => documentStore.isAnalysing, (newValue) => {
      localStorage.setItem('isAnalysing', newValue.toString())
      if (newValue) {
        startTimer()
      } else if (!isExtracting.value) {
        stopTimer()
      }
    })

    // Ajouter un watcher pour surveiller les changements de localStorage
    window.addEventListener('storage', (event) => {
      if (event.key === 'isExtracting') {
        isExtracting.value = event.newValue === 'true'
        console.log('Mise à jour isExtracting:', isExtracting.value)
      }
    })

    // Surveiller les changements de isExtracting
    watch(isExtracting, (newValue) => {
      console.log('isExtracting changé:', newValue)
      if (newValue) {
        startTimer()
      } else if (!documentStore.isAnalysing) {
        stopTimer()
      }
    })

    // Restaurer l'état et démarrer le timer si nécessaire au montage
    onMounted(() => {
      themeStore.initTheme()
      if (documentStore.isAnalysing || isExtracting.value) {
        startTimer()
      }
    })

    // Nettoyer l'intervalle lors de la destruction du composant
    onBeforeUnmount(() => {
      clearInterval(timerInterval)
    })

    const getLoadingMessage = () => {
      // Vérifier d'abord si nous sommes en extraction
      if (documentStore.isExtracting) {
        const fileName = documentStore.taskItemName || ''
        console.log('Extraction en cours, fichier:', fileName)
        return fileName ? `Extraction du fichier "${fileName}"...` : 'Extraction en cours...'
      }
      
      // Sinon vérifier le type d'analyse
      const taskType = documentStore.taskType || ''
      const itemName = documentStore.taskItemName || ''
      console.log(`Type: ${taskType}, Item: ${itemName}`)
      
      if (taskType === 'resume' && itemName) {
        return `Génération du résumé pour "${itemName}"...`
      } else if (taskType === 'analyse' && itemName) {
        return `Analyse de document "${itemName}"...`
      } else if (taskType === 'description' && itemName) {
        return `Description de l'image "${itemName}"...`
      }
      
      return 'Analyse en cours...'
    }

    // Surveiller les changements de isExtracting et isAnalysing
    watch([() => documentStore.isExtracting, () => documentStore.isAnalysing], ([newExtracting, newAnalysing]) => {
      if (newExtracting || newAnalysing) {
        startTimer();
      } else {
        stopTimer();
      }
    });

    // Restaurer l'état depuis localStorage au montage
    onMounted(() => {
      themeStore.initTheme();
      
      // Restaurer l'état d'extraction
      const isExtractingFromStorage = localStorage.getItem('isExtracting') === 'true';
      if (isExtractingFromStorage) {
        const taskItemName = localStorage.getItem('taskItemName') || '';
        documentStore.startExtracting(taskItemName);
      }
      
      // Restaurer l'état d'analyse
      if (localStorage.getItem('isAnalysing') === 'true') {
        documentStore.startAnalysing(
          localStorage.getItem('taskType') || '',
          localStorage.getItem('taskItemName') || ''
        );
      }
      
      // Démarrer le timer si nécessaire
      if (documentStore.isExtracting || documentStore.isAnalysing) {
        startTimer();
      }
    });

    return {
      documentStore,
      timer,
      isExtracting,
      getLoadingMessage
    }
  }
}
</script>

<template>
  <!-- Barre de chargement en bas avec timer incrémenté -->
  <div v-if="documentStore.isExtracting || documentStore.isAnalysing" class="loading-bar">
    <div class="loading-progress"></div>
    <p>
      {{ getLoadingMessage() }}
      <span>({{ timer }}s écoulées)</span>
    </p>
  </div>
  <div class="app-wrapper">
    <Header />
    <main class="main-content">
      <router-view></router-view>
    </main>
    <!-- <Footer /> -->
  </div>
</template>

<style>
:root {
  --primary-color: #7C3AED;
  --background-color: #ffffff;
  --text-color: #333333;
  --secondary-background: #f5f5f5;
  --border-color: #e0e0e0;
}

.dark-theme {
  --primary-color: #7445c5;
  --background-color: #121212;
  --text-color: #ffffff;
  --secondary-background: #1e1e1e;
  --border-color: #333333;
}

body {
  background-color: var(--background-color);
  color: var(--text-color);
  transition: background-color 0.3s, color 0.3s;
}

.app-wrapper {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--background-color);
}

.main-content {
  flex: 1;
}

.loading-bar {
  position: fixed;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 48px;
  background: #e2e8f0; /* Rectangle gris */
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
  color: #4b5563; /* Couleur légèrement plus claire pour le timer */
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
</style>






