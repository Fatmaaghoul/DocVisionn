<template>
  <div class="document-viewer">
    <div class="document-container">
      <div v-if="document.fileUrl" class="document-frame">
        <iframe :src="document.fileUrl" frameborder="0" width="100%" height="100%"></iframe>
      </div>
      <div v-else class="no-document">
        <i class="bi bi-file-earmark-text"></i>
        <p>Aucun aperçu disponible</p>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  document: {
    type: Object,
    required: true
  }
})
</script>

<style scoped>
.document-viewer {
  position: fixed;
  right: 0;
  /* top: 0; */
  width: 50%;
  height: 100vh;
  margin-right: 0;
  margin-top: 10px;
  background-color: #fff;
  border-left: 1px solid #ddd;
  overflow-y: auto;
  padding: 20px;
  box-sizing: border-box;
  z-index: 50;
}

.document-container {
  background: #fefefe;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  height: 100%;
  overflow: hidden;
}

.document-frame {
  width: 100%;
  height: 100%;
}

.no-document {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #6c757d;
}

.no-document i {
  font-size: 3rem;
  margin-bottom: 15px;
}

@media (max-width: 992px) {
  .document-viewer {
    position: static;
    width: 100%;
    height: auto;
    border-left: none;
    border-top: 1px solid #ddd;
  }

  .document-container {
    height: 500px;
  }
}

@media (max-width: 768px) {
  .document-container {
    height: 400px;
  }
}
</style>