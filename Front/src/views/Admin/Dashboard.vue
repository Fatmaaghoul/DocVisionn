<template>
  <div class="dashboard-container">
    <!-- Cartes de statistiques -->
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-icon users">
          <i class="bi bi-people-fill"></i>
        </div>
        <div class="stat-content">
          <h3 class="stat-value">{{ stats.totalUsers }}</h3>
          <p class="stat-label">Utilisateurs totaux</p>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon documents">
          <i class="bi bi-file-earmark-text-fill"></i>
        </div>
        <div class="stat-content">
          <h3 class="stat-value">{{ stats.totalDocuments }}</h3>
          <p class="stat-label">Documents totaux</p>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon storage">
          <i class="bi bi-hdd-fill"></i>
        </div>
        <div class="stat-content">
          <h3 class="stat-value">{{ formatStorage(stats.totalStorage) }}/{{ formatStorage(stats.maxStorage) }}</h3>
          <p class="stat-label">Stockage utilisé</p>
        </div>
      </div>
    </div>

    <!-- Section des graphiques -->
    <div class="charts-grid">
      <div class="chart-card modern">
        <div class="chart-header">
          <h3>Ajout des documents</h3>
          <div class="time-filter">
           
          </div>
        </div>
        <div class="chart-content">
          <canvas ref="barChart"></canvas>
        </div>
      </div>

      <div class="chart-card modern">
        <div class="chart-header">
          <h3>Utilisation du stockage</h3>
          <div class="chart-legend">
            <span class="legend-item">
              <span class="legend-color used"></span>
              Utilisé ({{ storagePercentage }}%)
            </span>
            <span class="legend-item">
              <span class="legend-color free"></span>
              Libre ({{ 100 - storagePercentage }}%)
            </span>
          </div>
        </div>
        <div class="chart-content">
          <canvas ref="pieChart"></canvas>
        </div>
      </div>

      <div class="chart-card modern">
        <div class="chart-header">
          <h3>Analyse des documents</h3>
          <div class="chart-legend">
            <span class="legend-item">
              <span class="legend-color analyzed"></span>
              Analysé ({{ analysisPercentage.analyzed }}%)
            </span>
            <span class="legend-item">
              <span class="legend-color not-analyzed"></span>
              Non analysé ({{ analysisPercentage.notAnalyzed }}%)
            </span>
          </div>
        </div>
        <div class="chart-content">
          <canvas ref="analysisChart"></canvas>
        </div>
      </div>
    </div>

    <!-- Activité récente -->
    <div class="activity-card">
      <div class="activity-header">
        <h3>Activité récente</h3>
        <button class="view-all-btn" @click="toggleShowAll">
          {{ showAllActivities ? 'Voir moins' : 'Voir tout' }}
          <i class="bi" :class="showAllActivities ? 'bi-chevron-up' : 'bi-chevron-down'"></i>
        </button>
      </div>
      <div class="activity-list">
        <div v-for="activity in recentActivities" :key="activity.id" class="activity-item">
          <div class="activity-icon" :class="activity.type">
            <i class="bi" :class="getActivityIcon(activity.type)"></i>
          </div>
          <div class="activity-details">
            <p class="activity-text">{{ activity.description }}</p>
            <span class="activity-time">{{ formatTime(activity.timestamp) }}</span>
          </div>
          <div class="activity-badge" v-if="activity.isNew">Nouveau</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import axios from 'axios'
import Chart from 'chart.js/auto'


// État réactif
const stats = ref({
  totalUsers: 0,
  userGrowth: 0,
  totalDocuments: 0,
  documentGrowth: 0,
  totalStorage: 0,
  maxStorage: 10,
  storageGrowth: 0,
  analyzedDocuments: 0,
  notAnalyzedDocuments: 0
})

const barChart = ref(null)
const pieChart = ref(null)
const analysisChart = ref(null)
const allActivities = ref([])
const showAllActivities = ref(false)
const barChartInstance = ref(null)
const pieChartInstance = ref(null)
const analysisChartInstance = ref(null)
const isLoading = ref(true)
const timePeriods = ['7j', '30j', '90j']
const selectedPeriod = ref('7j')

// Propriétés calculées
const recentActivities = computed(() => {
  const activities = [...allActivities.value]
    .filter(activity => activity.timestamp && !isNaN(new Date(activity.timestamp).getTime()))
    .sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp))
  
  return showAllActivities.value ? activities : activities.slice(0, 4)
})

const storagePercentage = computed(() => {
  if (stats.value.maxStorage === 0 || stats.value.totalStorage < 0) return 0
  return Math.round((stats.value.totalStorage / stats.value.maxStorage) * 100)
})

const analysisPercentage = computed(() => {
  const total = stats.value.analyzedDocuments + stats.value.notAnalyzedDocuments
  if (total === 0) return { analyzed: 0, notAnalyzed: 0 }
  return {
    analyzed: Math.round((stats.value.analyzedDocuments / total) * 100),
    notAnalyzed: Math.round((stats.value.notAnalyzedDocuments / total) * 100)
  }
})

// Fonctions utilitaires
const formatStorage = (value) => {
  if (value >= 1000) return `${(value / 1000).toFixed(1)} To`
  return `${value} Go`
}

const formatTime = (timestamp) => {
  const now = new Date()
  const date = new Date(timestamp)
  const diff = now - date
  const minutes = Math.floor(diff / 1000 / 60)
  
  if (minutes < 60) {
    return `il y a ${minutes} min`
  } else if (minutes < 1440) {
    const hours = Math.floor(minutes / 60)
    return `il y a ${hours} h`
  } else {
    return date.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' })
  }
}

const getActivityIcon = (type) => {
  const icons = {
    upload: 'bi-cloud-upload',
    edit: 'bi-pencil-square',
    delete: 'bi-trash',
    share: 'bi-share'
  }
  return icons[type] || 'bi-info-circle'
}

const toggleShowAll = () => {
  showAllActivities.value = !showAllActivities.value
}

// Initialisation des graphiques
const initBarChart = (data) => {
  if (barChartInstance.value) {
    barChartInstance.value.destroy()
  }

  const ctx = barChart.value.getContext('2d')
  const gradient = ctx.createLinearGradient(0, 0, 0, 300)
  gradient.addColorStop(0, 'rgba(78, 115, 223, 0.8)')
  gradient.addColorStop(1, 'rgba(78, 115, 223, 0.2)')

  barChartInstance.value = new Chart(ctx, {
    type: 'bar',
    data: {
      labels: data.map(item => item.label),
      datasets: [{
        data: data.map(item => item.count),
        backgroundColor: gradient,
        borderColor: 'rgba(78, 115, 223, 1)',
        borderWidth: 1,
        borderRadius: 12,
        borderSkipped: false,
        hoverBackgroundColor: 'rgba(78, 115, 223, 0.7)'
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          titleFont: { size: 14, weight: 'bold' },
          bodyFont: { size: 12 },
          padding: 12,
          cornerRadius: 8,
          callbacks: {
            label: (context) => `${context.parsed.y} documents`
          }
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          grid: { 
            color: 'rgba(0, 0, 0, 0.05)',
            drawBorder: false
          },
          ticks: { 
            color: '#6c757d',
            font: { size: 12 }
          }
        },
        x: {
          grid: { 
            display: false,
            drawBorder: false
          },
          ticks: { 
            color: '#6c757d',
            font: { size: 12 }
          }
        }
      },
      animation: {
        duration: 1000,
        easing: 'easeOutQuart'
      }
    }
  })
}

const initPieChart = () => {
  if (pieChartInstance.value) {
    pieChartInstance.value.destroy()
  }

  const ctx = pieChart.value.getContext('2d')
  pieChartInstance.value = new Chart(ctx, {
    type: 'doughnut',
    data: {
      labels: ['Utilisé', 'Libre'],
      datasets: [{
        data: [storagePercentage.value, 100 - storagePercentage.value],
        backgroundColor: [
          'rgba(78, 115, 223, 0.8)',
          'rgba(224, 224, 224, 0.5)'
        ],
        borderColor: [
          'rgba(78, 115, 223, 1)',
          'rgba(224, 224, 224, 1)'
        ],
        borderWidth: 1,
        hoverOffset: 8
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '75%',
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          bodyFont: { size: 12 },
          padding: 10,
          cornerRadius: 8,
          callbacks: {
            label: (context) => `${context.label}: ${context.parsed}%`
          }
        }
      },
      animation: {
        animateScale: true,
        animateRotate: true
      }
    }
  })
}

const initAnalysisChart = () => {
  if (analysisChartInstance.value) {
    analysisChartInstance.value.destroy()
  }

  const ctx = analysisChart.value.getContext('2d')
  analysisChartInstance.value = new Chart(ctx, {
    type: 'doughnut',
    data: {
      labels: ['Analysé', 'Non analysé'],
      datasets: [{
        data: [analysisPercentage.value.analyzed, analysisPercentage.value.notAnalyzed],
        backgroundColor: [
          'rgba(28, 200, 138, 0.8)', // Vert pour analysé
          'rgba(231, 74, 59, 0.8)'   // Rouge pour non analysé
        ],
        borderColor: [
          'rgba(28, 200, 138, 1)',
          'rgba(231, 74, 59, 1)'
        ],
        borderWidth: 1,
        hoverOffset: 8
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '75%',
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          bodyFont: { size: 12 },
          padding: 10,
          cornerRadius: 8,
          callbacks: {
            label: (context) => `${context.label}: ${context.parsed}%`
          }
        }
      },
      animation: {
        animateScale: true,
        animateRotate: true
      }
    }
  })
}

const changePeriod = (period) => {
  selectedPeriod.value = period
  // Ici vous pourriez ajouter la logique pour recharger les données
  // en fonction de la période sélectionnée
}

// Appels API
const fetchUsers = async () => {
  try {
    const response = await axios.get(`/api/users/all`)
    const users = response.data
    stats.value.totalUsers = users.length
    stats.value.userGrowth = Math.round(Math.random() * 20)
  } catch (error) {
    console.error('Erreur lors de la récupération des utilisateurs :', error)
  }
}

const fetchCloudinaryStorage = async () => {
  try {
    const response = await axios.get(`/api/admin/documents/storageCloud`, {
      timeout: 10000
    })
    if (typeof response.data.totalStorage !== 'number' || typeof response.data.maxStorage !== 'number') {
      throw new Error('Données de stockage invalides reçues')
    }
    stats.value.totalStorage = response.data.totalStorage
    stats.value.maxStorage = response.data.maxStorage > 0 ? response.data.maxStorage : 10
    stats.value.storageGrowth = Math.round(Math.random() * 5)
  } catch (error) {
    console.error('Erreur lors de la récupération des données de stockage :', error)
    stats.value.totalStorage = 0
    stats.value.maxStorage = 10
  }
}

const fetchDocuments = async () => {
  try {
    const response = await axios.get(`/api/admin/documents`)
    const documents = response.data
    stats.value.totalDocuments = documents.length
    stats.value.documentGrowth = Math.round(Math.random() * 10)
    stats.value.analyzedDocuments = documents.filter(doc => doc.isAnalysed).length
    stats.value.notAnalyzedDocuments = documents.filter(doc => !doc.isAnalysed).length
    return documents
  } catch (error) {
    console.error('Erreur lors de la récupération des documents :', error)
    return []
  }
}

const fetchRecentActivities = async (documents) => {
  try {
    // Transformer les données des documents en format d'activité (upload et edit)
    const activities = await Promise.all(
      documents.flatMap(async (doc) => {
        const activityItems = []
        const userResponse = await axios.get(`/api/admin/documents/${doc.id}/user`).catch(() => ({
          data: { userName: 'Utilisateur' }
        }))
        const user = userResponse.data

        // Activité de téléversement
        if (doc.uploadDate && !isNaN(new Date(doc.uploadDate).getTime())) {
          activityItems.push({
            id: `upload-${doc.id}`,
            type: 'upload',
            description: `${user.userName || 'Utilisateur'} a ajouté le document "${doc.name}"`,
            timestamp: doc.uploadDate,
            isNew: new Date() - new Date(doc.uploadDate) < 24 * 60 * 60 * 1000
          })
        }

        // Activité de modification
        if (doc.modifiedAt && !isNaN(new Date(doc.modifiedAt).getTime())) {
          activityItems.push({
            id: `edit-${doc.id}`,
            type: 'edit',
            description: `${user.userName || 'Utilisateur'} a modifié le document "${doc.name}"`,
            timestamp: doc.modifiedAt,
            isNew: new Date() - new Date(doc.modifiedAt) < 24 * 60 * 60 * 1000
          })
        }

        return activityItems
      })
    )

    allActivities.value = activities.flat()
  } catch (error) {
    console.error('Erreur lors de la récupération des activités :', error)
  }
}

const prepareChartData = (documents) => {
  const now = new Date()
  const bins = Array(7).fill().map((_, i) => {
    const date = new Date(now)
    date.setDate(now.getDate() - (6 - i))
    return {
      startDate: date,
      label: date.toLocaleDateString('fr-FR', { weekday: 'short' }),
      count: 0
    }
  })
  
  documents.forEach(doc => {
    if (!doc.uploadDate) return
    const docDate = new Date(doc.uploadDate)
    if (isNaN(docDate.getTime())) return
    
    for (const bin of bins) {
      if (
        docDate.getDate() === bin.startDate.getDate() &&
        docDate.getMonth() === bin.startDate.getMonth() &&
        docDate.getFullYear() === bin.startDate.getFullYear()
      ) {
        bin.count++
        break
      }
    }
  })
  
  return bins
}

// Récupération initiale des données
const fetchData = async () => {
  isLoading.value = true
  try {
    const documents = await fetchDocuments()
    await Promise.all([
      fetchUsers(),
      fetchCloudinaryStorage(),
      fetchRecentActivities(documents)
    ])
    
    const chartData = prepareChartData(documents)
    initBarChart(chartData)
    initPieChart()
    initAnalysisChart()
  } catch (error) {
    console.error('Erreur lors du chargement des données:', error)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  axios.interceptors.request.use(config => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  })

  fetchData()
})

// Mise à jour des graphiques camembert quand les pourcentages changent
watch(storagePercentage, () => {
  if (pieChartInstance.value) {
    pieChartInstance.value.data.datasets[0].data = [storagePercentage.value, 100 - storagePercentage.value]
    pieChartInstance.value.update()
  }
})

watch(analysisPercentage, () => {
  if (analysisChartInstance.value) {
    analysisChartInstance.value.data.datasets[0].data = [
      analysisPercentage.value.analyzed,
      analysisPercentage.value.notAnalyzed
    ]
    analysisChartInstance.value.update()
  }
})
</script>

<style scoped>
.dashboard-container {
  padding: 1.5rem;
  max-width: 1600px;
  margin: 0 auto;
  position: relative;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.8);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  font-size: 1.2rem;
  color: #4e73df;
}

/* Grille des statistiques */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  align-items: center;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.03);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  position: relative;
  overflow: hidden;
  border: 1px solid rgba(0, 0, 0, 0.03);
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.08);
}

.stat-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
}

.stat-card:nth-child(1)::before { background: linear-gradient(to bottom, #4e73df, #224abe); }
.stat-card:nth-child(2)::before { background: linear-gradient(to bottom, #1cc88a, #13855c); }
.stat-card:nth-child(3)::before { background: linear-gradient(to bottom, #f6c23e, #dda20a); }

.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 1rem;
  font-size: 1.5rem;
  color: white;
}

.stat-icon.users { background: #4e73df; }
.stat-icon.documents { background: #1cc88a; }
.stat-icon.storage { background: #f6c23e; }

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 1.75rem;
  font-weight: 700;
  margin: 0;
  color: #2c3e50;
  font-family: 'Inter', sans-serif;
}

.stat-label {
  margin: 0;
  color: #6c757d;
  font-size: 0.875rem;
  font-weight: 500;
}

.stat-trend {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.875rem;
  padding: 0.35rem 0.75rem;
  border-radius: 20px;
  font-weight: 600;
}

.stat-trend i {
  font-size: 0.8rem;
}

.stat-trend.positive {
  color: #1cc88a;
  background: rgba(28, 200, 138, 0.1);
}

.stat-trend.negative {
  color: #e74a3b;
  background: rgba(231, 74, 59, 0.1);
}

.stat-trend.neutral {
  color: #858796;
  background: rgba(133, 135, 150, 0.1);
}

/* Grille des graphiques */
.charts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(450px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.chart-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.03);
  border: 1px solid rgba(0, 0, 0, 0.03);
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.chart-header h3 {
  margin: 0;
  font-size: 1.25rem;
  color: #2c3e50;
  font-weight: 600;
}

.time-filter {
  display: flex;
  gap: 0.5rem;
}

.time-btn {
  padding: 0.35rem 0.75rem;
  border: 1px solid #e0e0e0;
  background: none;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 500;
  color: #6c757d;
  cursor: pointer;
  transition: all 0.2s ease;
}

.time-btn.active {
  background: #4e73df;
  border-color: #4e73df;
  color: white;
}

.time-btn:hover:not(.active) {
  background: #f8f9fc;
}

.chart-legend {
  display: flex;
  gap: 1.5rem;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: #6c757d;
  font-weight: 500;
}

.legend-color {
  width: 12px;
  height: 12px;
  border-radius: 3px;
}

.legend-color.used { background: #4e73df; }
.legend-color.free { background: #e0e0e0; }
.legend-color.analyzed { background: #1cc88a; }
.legend-color.not-analyzed { background: #e74a3b; }

.chart-content {
  height: 300px;
  position: relative;
}

/* Carte d'activité */
.activity-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.03);
  border: 1px solid rgba(0, 0, 0, 0.03);
}

.activity-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.activity-header h3 {
  margin: 0;
  font-size: 1.25rem;
  color: #2c3e50;
  font-weight: 600;
}

.view-all-btn {
  padding: 0.5rem 1rem;
  border: none;
  background: #f8f9fc;
  color: #4e73df;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 600;
  border-radius: 6px;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.2s ease;
}

.view-all-btn:hover {
  background: #e9ecef;
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.activity-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  border-radius: 8px;
  background: #f8f9fc;
  transition: all 0.2s ease;
  position: relative;
}

.activity-item:hover {
  background: #e9ecef;
  transform: translateX(2px);
}

.activity-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  color: white;
  flex-shrink: 0;
}

.activity-icon.upload { background: #4e73df; }
.activity-icon.edit { background: #1cc88a; }
.activity-icon.delete { background: #e74a3b; }
.activity-icon.share { background: #f6c23e; }

.activity-details {
  flex: 1;
  min-width: 0;
}

.activity-text {
  margin: 0;
  color: #2c3e50;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.activity-time {
  font-size: 0.75rem;
  color: #6c757d;
  display: block;
  margin-top: 0.25rem;
}

.activity-badge {
  position: absolute;
  top: -6px;
  right: -6px;
  background: #e74a3b;
  color: white;
  font-size: 0.65rem;
  font-weight: 600;
  padding: 0.15rem 0.5rem;
  border-radius: 10px;
}

/* Responsive */
@media (max-width: 1024px) {
  .charts-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .dashboard-container {
    padding: 1rem;
  }
  
  .chart-content {
    height: 250px;
  }
  
  .activity-text {
    white-space: normal;
  }
}

@media (max-width: 480px) {
  .chart-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
  
  .time-filter {
    align-self: flex-end;
  }
}

.chart-card.modern {
  background: white;
  border-radius: 16px;
  padding: 1.5rem;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.03);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.chart-card.modern:hover {
  transform: translateY(-5px);
  box-shadow: 0 12px 28px rgba(0, 0, 0, 0.1);
}

.chart-header h3 {
  margin: 0;
  font-size: 1.25rem;
  color: #2c3e50;
  font-weight: 600;
  font-family: 'Inter', sans-serif;
}

.time-filter {
  display: flex;
  gap: 0.5rem;
  background: rgba(241, 243, 246, 0.6);
  padding: 0.25rem;
  border-radius: 12px;
}

.time-btn {
  padding: 0.35rem 0.75rem;
  border: none;
  background: transparent;
  border-radius: 8px;
  font-size: 0.75rem;
  font-weight: 500;
  color: #6c757d;
  cursor: pointer;
  transition: all 0.2s ease;
  font-family: 'Inter', sans-serif;
}

.time-btn.active {
  background: white;
  color: #4e73df;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
  font-weight: 600;
}

.time-btn:hover:not(.active) {
  color: #4e73df;
}

.chart-legend {
  display: flex;
  gap: 1.5rem;
  background: rgba(241, 243, 246, 0.6);
  padding: 0.5rem 1rem;
  border-radius: 12px;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: #6c757d;
  font-weight: 500;
  font-family: 'Inter', sans-serif;
}

.legend-color {
  width: 12px;
  height: 12px;
  border-radius: 3px;
}

.legend-color.used { 
  background: #4e73df;
  box-shadow: 0 2px 4px rgba(78, 115, 223, 0.3);
}

.legend-color.free { 
  background: #e0e0e0;
  box-shadow: 0 2px 4px rgba(224, 224, 224, 0.3);
}

.legend-color.analyzed { 
  background: #1cc88a;
  box-shadow: 0 2px 4px rgba(28, 200, 138, 0.3);
}

.legend-color.not-analyzed { 
  background: #e74a3b;
  box-shadow: 0 2px 4px rgba(231, 74, 59, 0.3);
}

.chart-content {
  height: 300px;
  position: relative;
  margin-top: 1.5rem;
}

/* Animation pour les cartes */
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translate 0; }
}

.chart-card.modern {
  animation: fadeIn 0.6s ease-out forwards;
}

.chart-card.modern:nth-child(2) {
  animation-delay: 0.2s;
}

.chart-card.modern:nth-child(3) {
  animation-delay: 0.4s;
}

/* Responsive */
@media (max-width: 768px) {
  .chart-card.modern {
    padding: 1rem;
  }
  
  .chart-content {
    height: 250px;
  }
  
  .time-filter {
    margin-top: 0.5rem;
  }
}
</style>
