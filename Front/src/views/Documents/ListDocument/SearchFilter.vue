<template>
  <div class="search-container">
    <div class="search-input">
      <i class="bi bi-search"></i>
      <input
        type="text"
        :value="searchQuery"
        placeholder="Rechercher par nom, description, texte ou objets..."
        @input="$emit('update:search-query', $event.target.value)"
      />
      <button v-if="searchQuery" @click="$emit('clear-search')" class="clear-btn">
        <i class="bi bi-x"></i>
      </button>
    </div>
    <div class="filter-buttons">
      <button
        :class="['filter-btn', currentFilter === 'all' ? 'active' : '']"
        @click="$emit('update:filter', 'all')"
      >
        <i class="bi bi-files"></i>
        Tous
      </button>
      <button
        :class="['filter-btn', currentFilter === 'traiter' ? 'active' : '']"
        @click="$emit('update:filter', 'traiter')"
      >
        <i class="bi bi-check-circle"></i>
        Analysé
      </button>
      <button
        :class="['filter-btn', currentFilter === 'non-traiter' ? 'active' : '']"
        @click="$emit('update:filter', 'non-traiter')"
      >
        <i class="bi bi-x-circle"></i>
        Non Analysé
      </button>
      <div class="date-filter-wrapper">
        <button
          class="filter-btn date-filter-btn"
          @click="showDatePicker = !showDatePicker"
        >
          <i class="bi bi-calendar"></i>
          {{ dateRangeText }}
        </button>
        <div v-if="showDatePicker" class="date-picker-dropdown">
          <div class="date-input">
            <label>Date de début</label>
            <input
              type="date"
              :value="startDate"
              @input="$emit('update:start-date', $event.target.value)"
            />
          </div>
          <div class="date-input">
            <label>Date de fin</label>
            <input
              type="date"
              :value="endDate"
              @input="$emit('update:end-date', $event.target.value)"
            />
          </div>
          <button
            v-if="startDate || endDate"
            class="clear-date-btn"
            @click="clearDateAndClose"
          >
            Effacer le filtre de date
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'SearchFilter',
  props: {
    searchQuery: {
      type: String,
      required: true
    },
    currentFilter: {
      type: String,
      required: true
    },
    startDate: {
      type: String,
      default: ''
    },
    endDate: {
      type: String,
      default: ''
    }
  },
  data() {
    return {
      showDatePicker: false
    }
  },
  methods: {
    clearDateAndClose() {
      this.$emit('clear-date-filter')
      this.showDatePicker = false
    }
  },
  computed: {
    dateRangeText() {
      if (!this.startDate && !this.endDate) return 'Filtrer par date'
      if (this.startDate === this.endDate && this.startDate)
        return new Date(this.startDate).toLocaleDateString('fr-FR')
      return `${this.startDate ? new Date(this.startDate).toLocaleDateString('fr-FR') : ''} - ${this.endDate ? new Date(this.endDate).toLocaleDateString('fr-FR') : ''}`
    }
  },
  emits: ['update:search-query', 'update:filter', 'clear-search', 'update:start-date', 'update:end-date', 'clear-date-filter']
}
</script>

<style scoped>
.search-container {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 2rem;
  flex-wrap: wrap;
}

.search-input {
  position: relative;
  flex: 1;
  min-width: 250px;
  max-width: 500px;
}

.search-input input {
  width: 100%;
  padding: 0.75rem 1rem 0.75rem 2.5rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.3s ease;
}

.search-input input:focus {
  outline: none;
  border-color: #0d6efd;
  box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.1);
}

.search-input i {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #6c757d;
}

.clear-btn {
  position: absolute;
  right: 1rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #6c757d;
  cursor: pointer;
  font-size: 1.1rem;
}

.filter-buttons {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.filter-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: white;
  color: #666;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-btn i {
  font-size: 1rem;
}

.filter-btn:hover {
  background: #f8f9fa;
  border-color: #0d6efd;
  color: #0d6efd;
}

.filter-btn.active {
  background: #0d6efd;
  border-color: #0d6efd;
  color: white;
}

.date-filter-wrapper {
  position: relative;
}

.date-filter-btn {
  min-width: 150px;
}

.date-picker-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  background: white;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 1rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  z-index: 1000;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-top: 0.5rem;
}

.date-input {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.date-input label {
  font-size: 0.85rem;
  color: #666;
}

.date-input input {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.9rem;
}

.date-input input:focus {
  outline: none;
  border-color: #0d6efd;
}

.clear-date-btn {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background: #f8f9fa;
  color: #666;
  font-size: 0.9rem;
  cursor: pointer;
  text-align: center;
}

.clear-date-btn:hover {
  background: #e9ecef;
  border-color: #0d6efd;
  color: #0d6efd;
}

@media (max-width: 768px) {
  .search-container {
    flex-direction: column;
    align-items: stretch;
  }

  .search-input {
    max-width: none;
  }

  .filter-buttons {
    justify-content: center;
    flex-wrap: wrap;
  }

  .date-picker-dropdown {
    position: static;
    width: 100%;
  }
}
</style>
