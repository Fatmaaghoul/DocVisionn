<!-- DocumentGrid.vue -->
<template>
    <div class="documents-grid">
      <div class="document-card add-document-card" @click="$emit('show-add-modal')">
        <div class="document-content add-document-card-content">
          <div class="document-icon">
            <i class="bi bi-plus-circle add-icon"></i>
          </div>
          <div class="document-info">
            <h3 class="document-title">Ajouter un nouveau document</h3>
          </div>
        </div>
      </div>
      <DocumentCard
        v-for="doc in documents"
        :key="doc.id"
        :document="doc"
        @navigate-to-content="$emit('navigate-to-content', $event)"
        @download-document="$emit('download-document', $event)"
        @delete-document="$emit('delete-document', $event)"
      />
    </div>
  </template>
  
  <script>
  import DocumentCard from './DocumentCard.vue'
  
  export default {
    name: 'DocumentGrid',
    components: {
      DocumentCard
    },
    props: {
      documents: {
        type: Array,
        required: true
      }
    },
    emits: ['show-add-modal', 'navigate-to-content', 'download-document', 'delete-document']
  }
  </script>
  
  <style scoped>
  .documents-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 1.5rem;
  }
  
  .document-card {
    position: relative;
    background: white;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    overflow: hidden;
    transition: all 0.3s ease;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
  }
  
  .document-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
  }
  
  .add-document-card {
    cursor: pointer;
    border: 2px dashed #dee2e6;
    background-color: #f8f9fa;
  }
  
  .add-document-card:hover {
    border-color: #0d6efd;
    background-color: #f0f7ff;
  }
  
  .add-document-card-content {
    margin-top: 50px;
  }
  
  .add-icon {
    color: #0d6efd;
    font-size: 2rem;
    margin-top: -10px;
  }
  
  .document-content {
    padding: 1.5rem;
    display: flex;
    gap: 1rem;
  }
  
  .document-icon {
    font-size: 2rem;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  
  .document-info {
    flex: 1;
  }
  
  .document-title {
    margin: 0 0 0.5rem 0;
    font-size: 1.1rem;
    font-weight: 600;
    color: #2c3e50;
    word-break: break-word;
  }
  
  @media (max-width: 768px) {
    .documents-grid {
      grid-template-columns: 1fr;
    }
  }
  </style>